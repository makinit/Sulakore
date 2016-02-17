using System;
using System.IO;

using Sulakore.Compression.SevenZip.Compression.RangeCoder;

namespace Sulakore.Compression.SevenZip.Compression.LZMA
{
    public class LZMAEncoder : ICoder, ISetCoderProperties, IWriteCoderProperties
    {
        enum EMatchFinderType
        {
            BT2,
            BT4,
        };

        const uint kIfinityPrice = 0xFFFFFFF;

        static byte[] g_FastPos = new byte[1 << 11];

        static LZMAEncoder()
        {
            const byte kFastSlots = 22;
            int c = 2;
            g_FastPos[0] = 0;
            g_FastPos[1] = 1;
            for (byte slotFast = 2; slotFast < kFastSlots; slotFast++)
            {
                uint k = ((uint)1 << ((slotFast >> 1) - 1));
                for (uint j = 0; j < k; j++, c++)
                    g_FastPos[c] = slotFast;
            }
        }

        static uint GetPosSlot(uint pos)
        {
            if (pos < (1 << 11))
                return g_FastPos[pos];
            if (pos < (1 << 21))
                return (uint)(g_FastPos[pos >> 10] + 20);
            return (uint)(g_FastPos[pos >> 20] + 40);
        }

        static uint GetPosSlot2(uint pos)
        {
            if (pos < (1 << 17))
                return (uint)(g_FastPos[pos >> 6] + 12);
            if (pos < (1 << 27))
                return (uint)(g_FastPos[pos >> 16] + 32);
            return (uint)(g_FastPos[pos >> 26] + 52);
        }

        LZMABase.State _state = new LZMABase.State();
        byte _previousByte;
        uint[] _repDistances = new uint[LZMABase.kNumRepDistances];

        void BaseInit()
        {
            _state.Init();
            _previousByte = 0;
            for (uint i = 0; i < LZMABase.kNumRepDistances; i++)
                _repDistances[i] = 0;
        }

        const int kDefaultDictionaryLogSize = 22;
        const uint kNumFastBytesDefault = 0x20;

        class LiteralEncoder
        {
            public struct Encoder2
            {
                BitEncoder[] m_Encoders;

                public void Create() { m_Encoders = new BitEncoder[0x300]; }

                public void Init() { for (int i = 0; i < 0x300; i++) m_Encoders[i].Init(); }

                public void Encode(Encoder rangeEncoder, byte symbol)
                {
                    uint context = 1;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint)((symbol >> i) & 1);
                        m_Encoders[context].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public void EncodeMatched(Encoder rangeEncoder, byte matchByte, byte symbol)
                {
                    uint context = 1;
                    bool same = true;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint)((symbol >> i) & 1);
                        uint state = context;
                        if (same)
                        {
                            uint matchBit = (uint)((matchByte >> i) & 1);
                            state += ((1 + matchBit) << 8);
                            same = (matchBit == bit);
                        }
                        m_Encoders[state].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
                {
                    uint price = 0;
                    uint context = 1;
                    int i = 7;
                    if (matchMode)
                    {
                        for (; i >= 0; i--)
                        {
                            uint matchBit = (uint)(matchByte >> i) & 1;
                            uint bit = (uint)(symbol >> i) & 1;
                            price += m_Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                            context = (context << 1) | bit;
                            if (matchBit != bit)
                            {
                                i--;
                                break;
                            }
                        }
                    }
                    for (; i >= 0; i--)
                    {
                        uint bit = (uint)(symbol >> i) & 1;
                        price += m_Encoders[context].GetPrice(bit);
                        context = (context << 1) | bit;
                    }
                    return price;
                }
            }

            Encoder2[] m_Coders;
            int m_NumPrevBits;
            int m_NumPosBits;
            uint m_PosMask;

            public void Create(int numPosBits, int numPrevBits)
            {
                if (m_Coders != null && m_NumPrevBits == numPrevBits && m_NumPosBits == numPosBits)
                    return;
                m_NumPosBits = numPosBits;
                m_PosMask = ((uint)1 << numPosBits) - 1;
                m_NumPrevBits = numPrevBits;
                uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                m_Coders = new Encoder2[numStates];
                for (uint i = 0; i < numStates; i++)
                    m_Coders[i].Create();
            }

            public void Init()
            {
                uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                for (uint i = 0; i < numStates; i++)
                    m_Coders[i].Init();
            }

            public Encoder2 GetSubCoder(uint pos, byte prevByte)
            { return m_Coders[((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits))]; }
        }

        class LenEncoder
        {
            BitEncoder _choice = new BitEncoder();
            BitEncoder _choice2 = new BitEncoder();
            BitTreeEncoder[] _lowCoder = new BitTreeEncoder[LZMABase.kNumPosStatesEncodingMax];
            BitTreeEncoder[] _midCoder = new BitTreeEncoder[LZMABase.kNumPosStatesEncodingMax];
            BitTreeEncoder _highCoder = new BitTreeEncoder(LZMABase.kNumHighLenBits);

            public LenEncoder()
            {
                for (uint posState = 0; posState < LZMABase.kNumPosStatesEncodingMax; posState++)
                {
                    _lowCoder[posState] = new BitTreeEncoder(LZMABase.kNumLowLenBits);
                    _midCoder[posState] = new BitTreeEncoder(LZMABase.kNumMidLenBits);
                }
            }

            public void Init(uint numPosStates)
            {
                _choice.Init();
                _choice2.Init();
                for (uint posState = 0; posState < numPosStates; posState++)
                {
                    _lowCoder[posState].Init();
                    _midCoder[posState].Init();
                }
                _highCoder.Init();
            }

            public void Encode(Encoder rangeEncoder, uint symbol, uint posState)
            {
                if (symbol < LZMABase.kNumLowLenSymbols)
                {
                    _choice.Encode(rangeEncoder, 0);
                    _lowCoder[posState].Encode(rangeEncoder, symbol);
                }
                else
                {
                    symbol -= LZMABase.kNumLowLenSymbols;
                    _choice.Encode(rangeEncoder, 1);
                    if (symbol < LZMABase.kNumMidLenSymbols)
                    {
                        _choice2.Encode(rangeEncoder, 0);
                        _midCoder[posState].Encode(rangeEncoder, symbol);
                    }
                    else
                    {
                        _choice2.Encode(rangeEncoder, 1);
                        _highCoder.Encode(rangeEncoder, symbol - LZMABase.kNumMidLenSymbols);
                    }
                }
            }

            public void SetPrices(uint posState, uint numSymbols, uint[] prices, uint st)
            {
                uint a0 = _choice.GetPrice0();
                uint a1 = _choice.GetPrice1();
                uint b0 = a1 + _choice2.GetPrice0();
                uint b1 = a1 + _choice2.GetPrice1();
                uint i = 0;
                for (i = 0; i < LZMABase.kNumLowLenSymbols; i++)
                {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = a0 + _lowCoder[posState].GetPrice(i);
                }
                for (; i < LZMABase.kNumLowLenSymbols + LZMABase.kNumMidLenSymbols; i++)
                {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = b0 + _midCoder[posState].GetPrice(i - LZMABase.kNumLowLenSymbols);
                }
                for (; i < numSymbols; i++)
                    prices[st + i] = b1 + _highCoder.GetPrice(i - LZMABase.kNumLowLenSymbols - LZMABase.kNumMidLenSymbols);
            }
        };

        const uint kNumLenSpecSymbols = LZMABase.kNumLowLenSymbols + LZMABase.kNumMidLenSymbols;

        class LenPriceTableEncoder : LenEncoder
        {
            uint[] _prices = new uint[LZMABase.kNumLenSymbols << LZMABase.kNumPosStatesBitsEncodingMax];
            uint _tableSize;
            uint[] _counters = new uint[LZMABase.kNumPosStatesEncodingMax];

            public void SetTableSize(uint tableSize) { _tableSize = tableSize; }

            public uint GetPrice(uint symbol, uint posState)
            {
                return _prices[posState * LZMABase.kNumLenSymbols + symbol];
            }

            void UpdateTable(uint posState)
            {
                SetPrices(posState, _tableSize, _prices, posState * LZMABase.kNumLenSymbols);
                _counters[posState] = _tableSize;
            }

            public void UpdateTables(uint numPosStates)
            {
                for (uint posState = 0; posState < numPosStates; posState++)
                    UpdateTable(posState);
            }

            public new void Encode(Encoder rangeEncoder, uint symbol, uint posState)
            {
                base.Encode(rangeEncoder, symbol, posState);
                if (--_counters[posState] == 0)
                    UpdateTable(posState);
            }
        }

        const uint kNumOpts = 1 << 12;
        class Optimal
        {
            public LZMABase.State State;

            public bool Prev1IsChar;
            public bool Prev2;

            public uint PosPrev2;
            public uint BackPrev2;

            public uint Price;
            public uint PosPrev;
            public uint BackPrev;

            public uint Backs0;
            public uint Backs1;
            public uint Backs2;
            public uint Backs3;

            public void MakeAsChar() { BackPrev = 0xFFFFFFFF; Prev1IsChar = false; }
            public void MakeAsShortRep() { BackPrev = 0; ; Prev1IsChar = false; }
            public bool IsShortRep() { return (BackPrev == 0); }
        };
        Optimal[] _optimum = new Optimal[kNumOpts];
        LZ.IMatchFinder _matchFinder = null;
        Encoder _rangeEncoder = new Encoder();

        BitEncoder[] _isMatch = new BitEncoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];
        BitEncoder[] _isRep = new BitEncoder[LZMABase.kNumStates];
        BitEncoder[] _isRepG0 = new BitEncoder[LZMABase.kNumStates];
        BitEncoder[] _isRepG1 = new BitEncoder[LZMABase.kNumStates];
        BitEncoder[] _isRepG2 = new BitEncoder[LZMABase.kNumStates];
        BitEncoder[] _isRep0Long = new BitEncoder[LZMABase.kNumStates << LZMABase.kNumPosStatesBitsMax];

        BitTreeEncoder[] _posSlotEncoder = new BitTreeEncoder[LZMABase.kNumLenToPosStates];

        BitEncoder[] _posEncoders = new BitEncoder[LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex];
        BitTreeEncoder _posAlignEncoder = new BitTreeEncoder(LZMABase.kNumAlignBits);

        LenPriceTableEncoder _lenEncoder = new LenPriceTableEncoder();
        LenPriceTableEncoder _repMatchLenEncoder = new LenPriceTableEncoder();

        LiteralEncoder _literalEncoder = new LiteralEncoder();

        uint[] _matchDistances = new uint[LZMABase.kMatchMaxLen * 2 + 2];

        uint _numFastBytes = kNumFastBytesDefault;
        uint _longestMatchLength;
        uint _numDistancePairs;

        uint _additionalOffset;

        uint _optimumEndIndex;
        uint _optimumCurrentIndex;

        bool _longestMatchWasFound;

        uint[] _posSlotPrices = new uint[1 << (LZMABase.kNumPosSlotBits + LZMABase.kNumLenToPosStatesBits)];
        uint[] _distancesPrices = new uint[LZMABase.kNumFullDistances << LZMABase.kNumLenToPosStatesBits];
        uint[] _alignPrices = new uint[LZMABase.kAlignTableSize];
        uint _alignPriceCount;

        uint _distTableSize = (kDefaultDictionaryLogSize * 2);

        int _posStateBits = 2;
        uint _posStateMask = (4 - 1);
        int _numLiteralPosStateBits = 0;
        int _numLiteralContextBits = 3;

        uint _dictionarySize = (1 << kDefaultDictionaryLogSize);
        uint _dictionarySizePrev = 0xFFFFFFFF;
        uint _numFastBytesPrev = 0xFFFFFFFF;

        long nowPos64;
        bool _finished;
        Stream _inStream;

        EMatchFinderType _matchFinderType = EMatchFinderType.BT4;
        bool _writeEndMark = false;

        bool _needReleaseMFStream;

        void Create()
        {
            if (_matchFinder == null)
            {
                LZ.BinTree bt = new LZ.BinTree();
                int numHashBytes = 4;
                if (_matchFinderType == EMatchFinderType.BT2)
                    numHashBytes = 2;
                bt.SetType(numHashBytes);
                _matchFinder = bt;
            }
            _literalEncoder.Create(_numLiteralPosStateBits, _numLiteralContextBits);

            if (_dictionarySize == _dictionarySizePrev && _numFastBytesPrev == _numFastBytes)
                return;
            _matchFinder.Create(_dictionarySize, kNumOpts, _numFastBytes, LZMABase.kMatchMaxLen + 1);
            _dictionarySizePrev = _dictionarySize;
            _numFastBytesPrev = _numFastBytes;
        }

        public LZMAEncoder()
        {
            for (int i = 0; i < kNumOpts; i++)
                _optimum[i] = new Optimal();
            for (int i = 0; i < LZMABase.kNumLenToPosStates; i++)
                _posSlotEncoder[i] = new BitTreeEncoder(LZMABase.kNumPosSlotBits);
        }

        void SetWriteEndMarkerMode(bool writeEndMarker)
        {
            _writeEndMark = writeEndMarker;
        }

        void Init()
        {
            BaseInit();
            _rangeEncoder.Init();

            uint i;
            for (i = 0; i < LZMABase.kNumStates; i++)
            {
                for (uint j = 0; j <= _posStateMask; j++)
                {
                    uint complexState = (i << LZMABase.kNumPosStatesBitsMax) + j;
                    _isMatch[complexState].Init();
                    _isRep0Long[complexState].Init();
                }
                _isRep[i].Init();
                _isRepG0[i].Init();
                _isRepG1[i].Init();
                _isRepG2[i].Init();
            }
            _literalEncoder.Init();
            for (i = 0; i < LZMABase.kNumLenToPosStates; i++)
                _posSlotEncoder[i].Init();
            for (i = 0; i < LZMABase.kNumFullDistances - LZMABase.kEndPosModelIndex; i++)
                _posEncoders[i].Init();

            _lenEncoder.Init((uint)1 << _posStateBits);
            _repMatchLenEncoder.Init((uint)1 << _posStateBits);

            _posAlignEncoder.Init();

            _longestMatchWasFound = false;
            _optimumEndIndex = 0;
            _optimumCurrentIndex = 0;
            _additionalOffset = 0;
        }

        void ReadMatchDistances(out uint lenRes, out uint numDistancePairs)
        {
            lenRes = 0;
            numDistancePairs = _matchFinder.GetMatches(_matchDistances);
            if (numDistancePairs > 0)
            {
                lenRes = _matchDistances[numDistancePairs - 2];
                if (lenRes == _numFastBytes)
                    lenRes += _matchFinder.GetMatchLen((int)lenRes - 1, _matchDistances[numDistancePairs - 1],
                        LZMABase.kMatchMaxLen - lenRes);
            }
            _additionalOffset++;
        }


        void MovePos(uint num)
        {
            if (num > 0)
            {
                _matchFinder.Skip(num);
                _additionalOffset += num;
            }
        }

        uint GetRepLen1Price(LZMABase.State state, uint posState)
        {
            return _isRepG0[state.Index].GetPrice0() +
                    _isRep0Long[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0();
        }

        uint GetPureRepPrice(uint repIndex, LZMABase.State state, uint posState)
        {
            uint price;
            if (repIndex == 0)
            {
                price = _isRepG0[state.Index].GetPrice0();
                price += _isRep0Long[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
            }
            else
            {
                price = _isRepG0[state.Index].GetPrice1();
                if (repIndex == 1)
                    price += _isRepG1[state.Index].GetPrice0();
                else
                {
                    price += _isRepG1[state.Index].GetPrice1();
                    price += _isRepG2[state.Index].GetPrice(repIndex - 2);
                }
            }
            return price;
        }

        uint GetRepPrice(uint repIndex, uint len, LZMABase.State state, uint posState)
        {
            uint price = _repMatchLenEncoder.GetPrice(len - LZMABase.kMatchMinLen, posState);
            return price + GetPureRepPrice(repIndex, state, posState);
        }

        uint GetPosLenPrice(uint pos, uint len, uint posState)
        {
            uint price;
            uint lenToPosState = LZMABase.GetLenToPosState(len);
            if (pos < LZMABase.kNumFullDistances)
                price = _distancesPrices[(lenToPosState * LZMABase.kNumFullDistances) + pos];
            else
                price = _posSlotPrices[(lenToPosState << LZMABase.kNumPosSlotBits) + GetPosSlot2(pos)] +
                    _alignPrices[pos & LZMABase.kAlignMask];
            return price + _lenEncoder.GetPrice(len - LZMABase.kMatchMinLen, posState);
        }

        uint Backward(out uint backRes, uint cur)
        {
            _optimumEndIndex = cur;
            uint posMem = _optimum[cur].PosPrev;
            uint backMem = _optimum[cur].BackPrev;
            do
            {
                if (_optimum[cur].Prev1IsChar)
                {
                    _optimum[posMem].MakeAsChar();
                    _optimum[posMem].PosPrev = posMem - 1;
                    if (_optimum[cur].Prev2)
                    {
                        _optimum[posMem - 1].Prev1IsChar = false;
                        _optimum[posMem - 1].PosPrev = _optimum[cur].PosPrev2;
                        _optimum[posMem - 1].BackPrev = _optimum[cur].BackPrev2;
                    }
                }
                uint posPrev = posMem;
                uint backCur = backMem;

                backMem = _optimum[posPrev].BackPrev;
                posMem = _optimum[posPrev].PosPrev;

                _optimum[posPrev].BackPrev = backCur;
                _optimum[posPrev].PosPrev = cur;
                cur = posPrev;
            }
            while (cur > 0);
            backRes = _optimum[0].BackPrev;
            _optimumCurrentIndex = _optimum[0].PosPrev;
            return _optimumCurrentIndex;
        }

        uint[] reps = new uint[LZMABase.kNumRepDistances];
        uint[] repLens = new uint[LZMABase.kNumRepDistances];


        uint GetOptimum(uint position, out uint backRes)
        {
            if (_optimumEndIndex != _optimumCurrentIndex)
            {
                uint lenRes = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
                backRes = _optimum[_optimumCurrentIndex].BackPrev;
                _optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
                return lenRes;
            }
            _optimumCurrentIndex = _optimumEndIndex = 0;

            uint lenMain, numDistancePairs;
            if (!_longestMatchWasFound)
            {
                ReadMatchDistances(out lenMain, out numDistancePairs);
            }
            else
            {
                lenMain = _longestMatchLength;
                numDistancePairs = _numDistancePairs;
                _longestMatchWasFound = false;
            }

            uint numAvailableBytes = _matchFinder.GetNumAvailableBytes() + 1;
            if (numAvailableBytes < 2)
            {
                backRes = 0xFFFFFFFF;
                return 1;
            }
            if (numAvailableBytes > LZMABase.kMatchMaxLen)
                numAvailableBytes = LZMABase.kMatchMaxLen;

            uint repMaxIndex = 0;
            uint i;
            for (i = 0; i < LZMABase.kNumRepDistances; i++)
            {
                reps[i] = _repDistances[i];
                repLens[i] = _matchFinder.GetMatchLen(0 - 1, reps[i], LZMABase.kMatchMaxLen);
                if (repLens[i] > repLens[repMaxIndex])
                    repMaxIndex = i;
            }
            if (repLens[repMaxIndex] >= _numFastBytes)
            {
                backRes = repMaxIndex;
                uint lenRes = repLens[repMaxIndex];
                MovePos(lenRes - 1);
                return lenRes;
            }

            if (lenMain >= _numFastBytes)
            {
                backRes = _matchDistances[numDistancePairs - 1] + LZMABase.kNumRepDistances;
                MovePos(lenMain - 1);
                return lenMain;
            }

            byte currentByte = _matchFinder.GetIndexByte(0 - 1);
            byte matchByte = _matchFinder.GetIndexByte((Int32)(0 - _repDistances[0] - 1 - 1));

            if (lenMain < 2 && currentByte != matchByte && repLens[repMaxIndex] < 2)
            {
                backRes = (uint)0xFFFFFFFF;
                return 1;
            }

            _optimum[0].State = _state;

            uint posState = (position & _posStateMask);

            _optimum[1].Price = _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0() +
                    _literalEncoder.GetSubCoder(position, _previousByte).GetPrice(!_state.IsCharState(), matchByte, currentByte);
            _optimum[1].MakeAsChar();

            uint matchPrice = _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
            uint repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();

            if (matchByte == currentByte)
            {
                uint shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
                if (shortRepPrice < _optimum[1].Price)
                {
                    _optimum[1].Price = shortRepPrice;
                    _optimum[1].MakeAsShortRep();
                }
            }

            uint lenEnd = ((lenMain >= repLens[repMaxIndex]) ? lenMain : repLens[repMaxIndex]);

            if (lenEnd < 2)
            {
                backRes = _optimum[1].BackPrev;
                return 1;
            }

            _optimum[1].PosPrev = 0;

            _optimum[0].Backs0 = reps[0];
            _optimum[0].Backs1 = reps[1];
            _optimum[0].Backs2 = reps[2];
            _optimum[0].Backs3 = reps[3];

            uint len = lenEnd;
            do
                _optimum[len--].Price = kIfinityPrice;
            while (len >= 2);

            for (i = 0; i < LZMABase.kNumRepDistances; i++)
            {
                uint repLen = repLens[i];
                if (repLen < 2)
                    continue;
                uint price = repMatchPrice + GetPureRepPrice(i, _state, posState);
                do
                {
                    uint curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
                    Optimal optimum = _optimum[repLen];
                    if (curAndLenPrice < optimum.Price)
                    {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = i;
                        optimum.Prev1IsChar = false;
                    }
                }
                while (--repLen >= 2);
            }

            uint normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();

            len = ((repLens[0] >= 2) ? repLens[0] + 1 : 2);
            if (len <= lenMain)
            {
                uint offs = 0;
                while (len > _matchDistances[offs])
                    offs += 2;
                for (; ; len++)
                {
                    uint distance = _matchDistances[offs + 1];
                    uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
                    Optimal optimum = _optimum[len];
                    if (curAndLenPrice < optimum.Price)
                    {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = distance + LZMABase.kNumRepDistances;
                        optimum.Prev1IsChar = false;
                    }
                    if (len == _matchDistances[offs])
                    {
                        offs += 2;
                        if (offs == numDistancePairs)
                            break;
                    }
                }
            }

            uint cur = 0;

            while (true)
            {
                cur++;
                if (cur == lenEnd)
                    return Backward(out backRes, cur);
                uint newLen;
                ReadMatchDistances(out newLen, out numDistancePairs);
                if (newLen >= _numFastBytes)
                {
                    _numDistancePairs = numDistancePairs;
                    _longestMatchLength = newLen;
                    _longestMatchWasFound = true;
                    return Backward(out backRes, cur);
                }
                position++;
                uint posPrev = _optimum[cur].PosPrev;
                LZMABase.State state;
                if (_optimum[cur].Prev1IsChar)
                {
                    posPrev--;
                    if (_optimum[cur].Prev2)
                    {
                        state = _optimum[_optimum[cur].PosPrev2].State;
                        if (_optimum[cur].BackPrev2 < LZMABase.kNumRepDistances)
                            state.UpdateRep();
                        else
                            state.UpdateMatch();
                    }
                    else
                        state = _optimum[posPrev].State;
                    state.UpdateChar();
                }
                else
                    state = _optimum[posPrev].State;
                if (posPrev == cur - 1)
                {
                    if (_optimum[cur].IsShortRep())
                        state.UpdateShortRep();
                    else
                        state.UpdateChar();
                }
                else
                {
                    uint pos;
                    if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
                    {
                        posPrev = _optimum[cur].PosPrev2;
                        pos = _optimum[cur].BackPrev2;
                        state.UpdateRep();
                    }
                    else
                    {
                        pos = _optimum[cur].BackPrev;
                        if (pos < LZMABase.kNumRepDistances)
                            state.UpdateRep();
                        else
                            state.UpdateMatch();
                    }
                    Optimal opt = _optimum[posPrev];
                    if (pos < LZMABase.kNumRepDistances)
                    {
                        if (pos == 0)
                        {
                            reps[0] = opt.Backs0;
                            reps[1] = opt.Backs1;
                            reps[2] = opt.Backs2;
                            reps[3] = opt.Backs3;
                        }
                        else if (pos == 1)
                        {
                            reps[0] = opt.Backs1;
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs2;
                            reps[3] = opt.Backs3;
                        }
                        else if (pos == 2)
                        {
                            reps[0] = opt.Backs2;
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs1;
                            reps[3] = opt.Backs3;
                        }
                        else
                        {
                            reps[0] = opt.Backs3;
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs1;
                            reps[3] = opt.Backs2;
                        }
                    }
                    else
                    {
                        reps[0] = (pos - LZMABase.kNumRepDistances);
                        reps[1] = opt.Backs0;
                        reps[2] = opt.Backs1;
                        reps[3] = opt.Backs2;
                    }
                }
                _optimum[cur].State = state;
                _optimum[cur].Backs0 = reps[0];
                _optimum[cur].Backs1 = reps[1];
                _optimum[cur].Backs2 = reps[2];
                _optimum[cur].Backs3 = reps[3];
                uint curPrice = _optimum[cur].Price;

                currentByte = _matchFinder.GetIndexByte(0 - 1);
                matchByte = _matchFinder.GetIndexByte((Int32)(0 - reps[0] - 1 - 1));

                posState = (position & _posStateMask);

                uint curAnd1Price = curPrice +
                    _isMatch[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice0() +
                    _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexByte(0 - 2)).
                    GetPrice(!state.IsCharState(), matchByte, currentByte);

                Optimal nextOptimum = _optimum[cur + 1];

                bool nextIsChar = false;
                if (curAnd1Price < nextOptimum.Price)
                {
                    nextOptimum.Price = curAnd1Price;
                    nextOptimum.PosPrev = cur;
                    nextOptimum.MakeAsChar();
                    nextIsChar = true;
                }

                matchPrice = curPrice + _isMatch[(state.Index << LZMABase.kNumPosStatesBitsMax) + posState].GetPrice1();
                repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();

                if (matchByte == currentByte &&
                    !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                {
                    uint shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
                    if (shortRepPrice <= nextOptimum.Price)
                    {
                        nextOptimum.Price = shortRepPrice;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsShortRep();
                        nextIsChar = true;
                    }
                }

                uint numAvailableBytesFull = _matchFinder.GetNumAvailableBytes() + 1;
                numAvailableBytesFull = Math.Min(kNumOpts - 1 - cur, numAvailableBytesFull);
                numAvailableBytes = numAvailableBytesFull;

                if (numAvailableBytes < 2)
                    continue;
                if (numAvailableBytes > _numFastBytes)
                    numAvailableBytes = _numFastBytes;
                if (!nextIsChar && matchByte != currentByte)
                {
                    // try Literal + rep0
                    uint t = Math.Min(numAvailableBytesFull - 1, _numFastBytes);
                    uint lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
                    if (lenTest2 >= 2)
                    {
                        LZMABase.State state2 = state;
                        state2.UpdateChar();
                        uint posStateNext = (position + 1) & _posStateMask;
                        uint nextRepMatchPrice = curAnd1Price +
                            _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1() +
                            _isRep[state2.Index].GetPrice1();
                        {
                            uint offset = cur + 1 + lenTest2;
                            while (lenEnd < offset)
                                _optimum[++lenEnd].Price = kIfinityPrice;
                            uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(
                                0, lenTest2, state2, posStateNext);
                            Optimal optimum = _optimum[offset];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur + 1;
                                optimum.BackPrev = 0;
                                optimum.Prev1IsChar = true;
                                optimum.Prev2 = false;
                            }
                        }
                    }
                }

                uint startLen = 2; // speed optimization 

                for (uint repIndex = 0; repIndex < LZMABase.kNumRepDistances; repIndex++)
                {
                    uint lenTest = _matchFinder.GetMatchLen(0 - 1, reps[repIndex], numAvailableBytes);
                    if (lenTest < 2)
                        continue;
                    uint lenTestTemp = lenTest;
                    do
                    {
                        while (lenEnd < cur + lenTest)
                            _optimum[++lenEnd].Price = kIfinityPrice;
                        uint curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState);
                        Optimal optimum = _optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = repIndex;
                            optimum.Prev1IsChar = false;
                        }
                    }
                    while (--lenTest >= 2);
                    lenTest = lenTestTemp;

                    if (repIndex == 0)
                        startLen = lenTest + 1;

                    // if (_maxMode)
                    if (lenTest < numAvailableBytesFull)
                    {
                        uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                        uint lenTest2 = _matchFinder.GetMatchLen((Int32)lenTest, reps[repIndex], t);
                        if (lenTest2 >= 2)
                        {
                            LZMABase.State state2 = state;
                            state2.UpdateRep();
                            uint posStateNext = (position + lenTest) & _posStateMask;
                            uint curAndLenCharPrice =
                                    repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState) +
                                    _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                    _literalEncoder.GetSubCoder(position + lenTest,
                                    _matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).GetPrice(true,
                                    _matchFinder.GetIndexByte((Int32)((Int32)lenTest - 1 - (Int32)(reps[repIndex] + 1))),
                                    _matchFinder.GetIndexByte((Int32)lenTest - 1));
                            state2.UpdateChar();
                            posStateNext = (position + lenTest + 1) & _posStateMask;
                            uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                            uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                            // for(; lenTest2 >= 2; lenTest2--)
                            {
                                uint offset = lenTest + 1 + lenTest2;
                                while (lenEnd < cur + offset)
                                    _optimum[++lenEnd].Price = kIfinityPrice;
                                uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                Optimal optimum = _optimum[cur + offset];
                                if (curAndLenPrice < optimum.Price)
                                {
                                    optimum.Price = curAndLenPrice;
                                    optimum.PosPrev = cur + lenTest + 1;
                                    optimum.BackPrev = 0;
                                    optimum.Prev1IsChar = true;
                                    optimum.Prev2 = true;
                                    optimum.PosPrev2 = cur;
                                    optimum.BackPrev2 = repIndex;
                                }
                            }
                        }
                    }
                }

                if (newLen > numAvailableBytes)
                {
                    newLen = numAvailableBytes;
                    for (numDistancePairs = 0; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2) ;
                    _matchDistances[numDistancePairs] = newLen;
                    numDistancePairs += 2;
                }
                if (newLen >= startLen)
                {
                    normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
                    while (lenEnd < cur + newLen)
                        _optimum[++lenEnd].Price = kIfinityPrice;

                    uint offs = 0;
                    while (startLen > _matchDistances[offs])
                        offs += 2;

                    for (uint lenTest = startLen; ; lenTest++)
                    {
                        uint curBack = _matchDistances[offs + 1];
                        uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
                        Optimal optimum = _optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = curBack + LZMABase.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }

                        if (lenTest == _matchDistances[offs])
                        {
                            if (lenTest < numAvailableBytesFull)
                            {
                                uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                                uint lenTest2 = _matchFinder.GetMatchLen((Int32)lenTest, curBack, t);
                                if (lenTest2 >= 2)
                                {
                                    LZMABase.State state2 = state;
                                    state2.UpdateMatch();
                                    uint posStateNext = (position + lenTest) & _posStateMask;
                                    uint curAndLenCharPrice = curAndLenPrice +
                                        _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                        _literalEncoder.GetSubCoder(position + lenTest,
                                        _matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).
                                        GetPrice(true,
                                        _matchFinder.GetIndexByte((Int32)lenTest - (Int32)(curBack + 1) - 1),
                                        _matchFinder.GetIndexByte((Int32)lenTest - 1));
                                    state2.UpdateChar();
                                    posStateNext = (position + lenTest + 1) & _posStateMask;
                                    uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << LZMABase.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                                    uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                    uint offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                        _optimum[++lenEnd].Price = kIfinityPrice;
                                    curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                    optimum = _optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price)
                                    {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = curBack + LZMABase.kNumRepDistances;
                                    }
                                }
                            }
                            offs += 2;
                            if (offs == numDistancePairs)
                                break;
                        }
                    }
                }
            }
        }

        bool ChangePair(uint smallDist, uint bigDist)
        {
            const int kDif = 7;
            return (smallDist < ((uint)(1) << (32 - kDif)) && bigDist >= (smallDist << kDif));
        }

        void WriteEndMarker(uint posState)
        {
            if (!_writeEndMark)
                return;

            _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 1);
            _isRep[_state.Index].Encode(_rangeEncoder, 0);
            _state.UpdateMatch();
            uint len = LZMABase.kMatchMinLen;
            _lenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
            uint posSlot = (1 << LZMABase.kNumPosSlotBits) - 1;
            uint lenToPosState = LZMABase.GetLenToPosState(len);
            _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);
            int footerBits = 30;
            uint posReduced = (((uint)1) << footerBits) - 1;
            _rangeEncoder.EncodeDirectBits(posReduced >> LZMABase.kNumAlignBits, footerBits - LZMABase.kNumAlignBits);
            _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & LZMABase.kAlignMask);
        }

        void Flush(uint nowPos)
        {
            ReleaseMFStream();
            WriteEndMarker(nowPos & _posStateMask);
            _rangeEncoder.FlushData();
            _rangeEncoder.FlushStream();
        }

        public void CodeOneBlock(out long inSize, out long outSize, out bool finished)
        {
            inSize = 0;
            outSize = 0;
            finished = true;

            if (_inStream != null)
            {
                _matchFinder.SetStream(_inStream);
                _matchFinder.Init();
                _needReleaseMFStream = true;
                _inStream = null;
                if (_trainSize > 0)
                    _matchFinder.Skip(_trainSize);
            }

            if (_finished)
                return;
            _finished = true;


            long progressPosValuePrev = nowPos64;
            if (nowPos64 == 0)
            {
                if (_matchFinder.GetNumAvailableBytes() == 0)
                {
                    Flush((uint)nowPos64);
                    return;
                }
                uint len, numDistancePairs; // it's not used
                ReadMatchDistances(out len, out numDistancePairs);
                uint posState = (uint)(nowPos64) & _posStateMask;
                _isMatch[(_state.Index << LZMABase.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 0);
                _state.UpdateChar();
                byte curByte = _matchFinder.GetIndexByte((Int32)(0 - _additionalOffset));
                _literalEncoder.GetSubCoder((uint)(nowPos64), _previousByte).Encode(_rangeEncoder, curByte);
                _previousByte = curByte;
                _additionalOffset--;
                nowPos64++;
            }
            if (_matchFinder.GetNumAvailableBytes() == 0)
            {
                Flush((uint)nowPos64);
                return;
            }
            while (true)
            {
                uint pos;
                uint len = GetOptimum((uint)nowPos64, out pos);

                uint posState = ((uint)nowPos64) & _posStateMask;
                uint complexState = (_state.Index << LZMABase.kNumPosStatesBitsMax) + posState;
                if (len == 1 && pos == 0xFFFFFFFF)
                {
                    _isMatch[complexState].Encode(_rangeEncoder, 0);
                    byte curByte = _matchFinder.GetIndexByte((Int32)(0 - _additionalOffset));
                    LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((uint)nowPos64, _previousByte);
                    if (!_state.IsCharState())
                    {
                        byte matchByte = _matchFinder.GetIndexByte((Int32)(0 - _repDistances[0] - 1 - _additionalOffset));
                        subCoder.EncodeMatched(_rangeEncoder, matchByte, curByte);
                    }
                    else
                        subCoder.Encode(_rangeEncoder, curByte);
                    _previousByte = curByte;
                    _state.UpdateChar();
                }
                else
                {
                    _isMatch[complexState].Encode(_rangeEncoder, 1);
                    if (pos < LZMABase.kNumRepDistances)
                    {
                        _isRep[_state.Index].Encode(_rangeEncoder, 1);
                        if (pos == 0)
                        {
                            _isRepG0[_state.Index].Encode(_rangeEncoder, 0);
                            if (len == 1)
                                _isRep0Long[complexState].Encode(_rangeEncoder, 0);
                            else
                                _isRep0Long[complexState].Encode(_rangeEncoder, 1);
                        }
                        else
                        {
                            _isRepG0[_state.Index].Encode(_rangeEncoder, 1);
                            if (pos == 1)
                                _isRepG1[_state.Index].Encode(_rangeEncoder, 0);
                            else
                            {
                                _isRepG1[_state.Index].Encode(_rangeEncoder, 1);
                                _isRepG2[_state.Index].Encode(_rangeEncoder, pos - 2);
                            }
                        }
                        if (len == 1)
                            _state.UpdateShortRep();
                        else
                        {
                            _repMatchLenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
                            _state.UpdateRep();
                        }
                        uint distance = _repDistances[pos];
                        if (pos != 0)
                        {
                            for (uint i = pos; i >= 1; i--)
                                _repDistances[i] = _repDistances[i - 1];
                            _repDistances[0] = distance;
                        }
                    }
                    else
                    {
                        _isRep[_state.Index].Encode(_rangeEncoder, 0);
                        _state.UpdateMatch();
                        _lenEncoder.Encode(_rangeEncoder, len - LZMABase.kMatchMinLen, posState);
                        pos -= LZMABase.kNumRepDistances;
                        uint posSlot = GetPosSlot(pos);
                        uint lenToPosState = LZMABase.GetLenToPosState(len);
                        _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);

                        if (posSlot >= LZMABase.kStartPosModelIndex)
                        {
                            int footerBits = (int)((posSlot >> 1) - 1);
                            uint baseVal = ((2 | (posSlot & 1)) << footerBits);
                            uint posReduced = pos - baseVal;

                            if (posSlot < LZMABase.kEndPosModelIndex)
                                RangeCoder.BitTreeEncoder.ReverseEncode(_posEncoders,
                                        baseVal - posSlot - 1, _rangeEncoder, footerBits, posReduced);
                            else
                            {
                                _rangeEncoder.EncodeDirectBits(posReduced >> LZMABase.kNumAlignBits, footerBits - LZMABase.kNumAlignBits);
                                _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & LZMABase.kAlignMask);
                                _alignPriceCount++;
                            }
                        }
                        uint distance = pos;
                        for (uint i = LZMABase.kNumRepDistances - 1; i >= 1; i--)
                            _repDistances[i] = _repDistances[i - 1];
                        _repDistances[0] = distance;
                        _matchPriceCount++;
                    }
                    _previousByte = _matchFinder.GetIndexByte((Int32)(len - 1 - _additionalOffset));
                }
                _additionalOffset -= len;
                nowPos64 += len;
                if (_additionalOffset == 0)
                {
                    // if (!_fastMode)
                    if (_matchPriceCount >= (1 << 7))
                        FillDistancesPrices();
                    if (_alignPriceCount >= LZMABase.kAlignTableSize)
                        FillAlignPrices();
                    inSize = nowPos64;
                    outSize = _rangeEncoder.GetProcessedSizeAdd();
                    if (_matchFinder.GetNumAvailableBytes() == 0)
                    {
                        Flush((uint)nowPos64);
                        return;
                    }

                    if (nowPos64 - progressPosValuePrev >= (1 << 12))
                    {
                        _finished = false;
                        finished = false;
                        return;
                    }
                }
            }
        }

        void ReleaseMFStream()
        {
            if (_matchFinder != null && _needReleaseMFStream)
            {
                _matchFinder.ReleaseStream();
                _needReleaseMFStream = false;
            }
        }

        private void SetOutStream(Stream stream) =>
            _rangeEncoder.SetStream(stream);

        private void ReleaseOutStream() =>
            _rangeEncoder.ReleaseStream();

        private void ReleaseStreams()
        {
            ReleaseMFStream();
            ReleaseOutStream();
        }


        const int kPropSize = 5;
        byte[] properties = new byte[kPropSize];

        uint _trainSize = 0;
        uint _matchPriceCount;
        uint[] tempPrices = new uint[LZMABase.kNumFullDistances];
        static string[] kMatchFinderIDs =
        {
            "BT2",
            "BT4",
        };


        private static int FindMatchFinder(string s)
        {
            for (int m = 0; m < kMatchFinderIDs.Length; m++)
                if (s == kMatchFinderIDs[m])
                    return m;
            return -1;
        }

        private void SetStreams(Stream inStream, Stream outStream, long inSize, long outSize)
        {
            _inStream = inStream;
            _finished = false;
            Create();
            SetOutStream(outStream);
            Init();

            // if (!_fastMode)
            {
                FillDistancesPrices();
                FillAlignPrices();
            }

            _lenEncoder.SetTableSize(_numFastBytes + 1 - LZMABase.kMatchMinLen);
            _lenEncoder.UpdateTables((uint)1 << _posStateBits);
            _repMatchLenEncoder.SetTableSize(_numFastBytes + 1 - LZMABase.kMatchMinLen);
            _repMatchLenEncoder.UpdateTables((uint)1 << _posStateBits);

            nowPos64 = 0;
        }
        public void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress progress)
        {
            _needReleaseMFStream = false;
            try
            {
                SetStreams(inStream, outStream, inSize, outSize);
                while (true)
                {
                    long processedInSize;
                    long processedOutSize;
                    bool finished;
                    CodeOneBlock(out processedInSize, out processedOutSize, out finished);
                    if (finished)
                        return;
                    if (progress != null)
                    {
                        progress.SetProgress(processedInSize, processedOutSize);
                    }
                }
            }
            finally
            {
                ReleaseStreams();
            }
        }

        private void FillAlignPrices()
        {
            for (uint i = 0; i < LZMABase.kAlignTableSize; i++)
                _alignPrices[i] = _posAlignEncoder.ReverseGetPrice(i);
            _alignPriceCount = 0;
        }
        private void FillDistancesPrices()
        {
            for (uint i = LZMABase.kStartPosModelIndex; i < LZMABase.kNumFullDistances; i++)
            {
                uint posSlot = GetPosSlot(i);
                int footerBits = (int)((posSlot >> 1) - 1);
                uint baseVal = ((2 | (posSlot & 1)) << footerBits);
                tempPrices[i] = BitTreeEncoder.ReverseGetPrice(_posEncoders,
                    baseVal - posSlot - 1, footerBits, i - baseVal);
            }

            for (uint lenToPosState = 0; lenToPosState < LZMABase.kNumLenToPosStates; lenToPosState++)
            {
                uint posSlot;
                BitTreeEncoder encoder = _posSlotEncoder[lenToPosState];

                uint st = (lenToPosState << LZMABase.kNumPosSlotBits);
                for (posSlot = 0; posSlot < _distTableSize; posSlot++)
                    _posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                for (posSlot = LZMABase.kEndPosModelIndex; posSlot < _distTableSize; posSlot++)
                    _posSlotPrices[st + posSlot] += ((((posSlot >> 1) - 1) - LZMABase.kNumAlignBits) << RangeCoder.BitEncoder.kNumBitPriceShiftBits);

                uint st2 = lenToPosState * LZMABase.kNumFullDistances;
                uint i;
                for (i = 0; i < LZMABase.kStartPosModelIndex; i++)
                    _distancesPrices[st2 + i] = _posSlotPrices[st + i];
                for (; i < LZMABase.kNumFullDistances; i++)
                    _distancesPrices[st2 + i] = _posSlotPrices[st + GetPosSlot(i)] + tempPrices[i];
            }
            _matchPriceCount = 0;
        }

        public void SetTrainSize(uint trainSize)
        {
            _trainSize = trainSize;
        }
        public void WriteCoderProperties(Stream stream)
        {
            properties[0] = (byte)((_posStateBits * 5 +
                _numLiteralPosStateBits) * 9 + _numLiteralContextBits);

            for (int i = 0; i < 4; i++)
            {
                properties[1 + i] =
                    (byte)((_dictionarySize >> (8 * i)) & 0xFF);
            }
            stream.Write(properties, 0, kPropSize);
        }
        public void SetCoderProperties(CoderPropID[] propIDs, object[] properties)
        {
            for (uint i = 0; i < properties.Length; i++)
            {
                object prop = properties[i];
                switch (propIDs[i])
                {
                    case CoderPropID.NumFastBytes:
                    {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        Int32 numFastBytes = (Int32)prop;
                        if (numFastBytes < 5 || numFastBytes > LZMABase.kMatchMaxLen)
                            throw new InvalidParamException();
                        _numFastBytes = (uint)numFastBytes;
                        break;
                    }
                    case CoderPropID.Algorithm:
                    {
                        /*
						if (!(prop is Int32))
							throw new InvalidParamException();
						Int32 maximize = (Int32)prop;
						_fastMode = (maximize == 0);
						_maxMode = (maximize >= 2);
						*/
                        break;
                    }
                    case CoderPropID.MatchFinder:
                    {
                        if (!(prop is String))
                            throw new InvalidParamException();
                        EMatchFinderType matchFinderIndexPrev = _matchFinderType;
                        int m = FindMatchFinder(((string)prop).ToUpper());
                        if (m < 0)
                            throw new InvalidParamException();
                        _matchFinderType = (EMatchFinderType)m;
                        if (_matchFinder != null && matchFinderIndexPrev != _matchFinderType)
                        {
                            _dictionarySizePrev = 0xFFFFFFFF;
                            _matchFinder = null;
                        }
                        break;
                    }
                    case CoderPropID.DictionarySize:
                    {
                        const int kDicLogSizeMaxCompress = 30;
                        if (!(prop is Int32))
                            throw new InvalidParamException(); ;
                        Int32 dictionarySize = (Int32)prop;
                        if (dictionarySize < (uint)(1 << LZMABase.kDicLogSizeMin) ||
                            dictionarySize > (uint)(1 << kDicLogSizeMaxCompress))
                            throw new InvalidParamException();
                        _dictionarySize = (uint)dictionarySize;
                        int dicLogSize;
                        for (dicLogSize = 0; dicLogSize < (uint)kDicLogSizeMaxCompress; dicLogSize++)
                            if (dictionarySize <= ((uint)(1) << dicLogSize))
                                break;
                        _distTableSize = (uint)dicLogSize * 2;
                        break;
                    }
                    case CoderPropID.PosStateBits:
                    {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        Int32 v = (Int32)prop;
                        if (v < 0 || v > (uint)LZMABase.kNumPosStatesBitsEncodingMax)
                            throw new InvalidParamException();
                        _posStateBits = (int)v;
                        _posStateMask = (((uint)1) << (int)_posStateBits) - 1;
                        break;
                    }
                    case CoderPropID.LitPosBits:
                    {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        Int32 v = (Int32)prop;
                        if (v < 0 || v > (uint)LZMABase.kNumLitPosStatesBitsEncodingMax)
                            throw new InvalidParamException();
                        _numLiteralPosStateBits = (int)v;
                        break;
                    }
                    case CoderPropID.LitContextBits:
                    {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        Int32 v = (Int32)prop;
                        if (v < 0 || v > (uint)LZMABase.kNumLitContextBitsMax)
                            throw new InvalidParamException(); ;
                        _numLiteralContextBits = (int)v;
                        break;
                    }
                    case CoderPropID.EndMarker:
                    {
                        if (!(prop is Boolean))
                            throw new InvalidParamException();
                        SetWriteEndMarkerMode((Boolean)prop);
                        break;
                    }
                    default:
                    throw new InvalidParamException();
                }
            }
        }
    }
}