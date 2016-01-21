using System.IO;
using System.Collections.Generic;

using Sulakore.Disassembler.ActionScript;

namespace Sulakore.Disassembler.IO
{
    /// <summary>
    /// Represnts a readable Shockwave Flash(SWF) file.
    /// </summary>
    public class FlashReader : BinaryReader
    {
        private readonly MemoryStream _flashStream;

        protected int BitPosition { get; set; }
        protected byte BitContainer { get; set; }

        public int Position
        {
            get { return (int)BaseStream.Position; }
            set { BaseStream.Position = value; }
        }
        public int Length => (int)BaseStream.Length;
        public bool IsDataAvailable => (Position >= Length);

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashReader"/> class based on the specified byte array and using UTF8-encoding.
        /// </summary>
        /// <param name="data">The byte array containing the data.</param>
        public FlashReader(byte[] data) :
            base(new MemoryStream(data.Length))
        {
            _flashStream = (MemoryStream)BaseStream;
            _flashStream.Write(data, 0, data.Length);
            Position = 0;
        }

        public int ReadUB(int bitCount)
        {
            if (bitCount == 0) return 0;

            if (BitPosition == 0)
                BitContainer = ReadByte();

            var result = 0;
            for (int bit = 0; bit < bitCount; bit++)
            {
                int nb = (BitContainer >> (7 - BitPosition)) & 1;
                result += (nb << (bitCount - 1 - bit));

                if (++BitPosition == 8)
                {
                    BitPosition = 0;

                    if (bit != (bitCount - 1))
                        BitContainer = ReadByte();
                }
            }
            return result;
        }
        public int ReadSB(int bitCount)
        {
            int result = ReadUB(bitCount);
            int shift = (32 - bitCount);

            return (result << shift) >> shift;
        }

        public uint ReadS24()
        {
            var value = (uint)(ReadByte() + (ReadByte() << 8) + (ReadByte() << 16));

            if ((value >> 23) == 1)
                value |= 0xff000000;

            return value;
        }
        public OPCode ReadOP()
        {
            return (OPCode)ReadByte();
        }

        public object[] ReadValues(OPCode op)
        {
            var values = new List<object>();
            switch (op)
            {
                case OPCode.AsType:
                case OPCode.Call:
                case OPCode.Coerce:
                case OPCode.Construct:
                case OPCode.ConstructSuper:
                case OPCode.DebugFile:
                case OPCode.DebugLine:
                case OPCode.DecLocal:
                case OPCode.DecLocal_i:
                case OPCode.DeleteProperty:
                case OPCode.Dxns:
                case OPCode.FindProperty:
                case OPCode.FindPropStrict:
                case OPCode.GetDescendants:
                case OPCode.GetGlobalSlot:
                case OPCode.GetLex:
                case OPCode.GetLocal:
                case OPCode.GetProperty:
                case OPCode.GetScopeObject:
                case OPCode.GetSlot:
                case OPCode.GetSuper:
                case OPCode.IncLocal:
                case OPCode.IncLocal_i:
                case OPCode.InitProperty:
                case OPCode.IsType:
                case OPCode.Kill:
                case OPCode.NewArray:
                case OPCode.NewCatch:
                case OPCode.NewClass:
                case OPCode.NewFunction:
                case OPCode.NewObject:
                case OPCode.PushDouble:
                case OPCode.PushInt:
                case OPCode.PushNamespace:
                case OPCode.PushShort:
                case OPCode.PushString:
                case OPCode.PushUInt:
                case OPCode.SetLocal:
                case OPCode.SetGlobalSlot:
                case OPCode.SetProperty:
                case OPCode.SetSlot:
                case OPCode.SetSuper:
                values.Add(Read7BitEncodedInt());
                break;

                case OPCode.CallMethod:
                case OPCode.CallProperty:
                case OPCode.CallPropLex:
                case OPCode.CallPropVoid:
                case OPCode.CallStatic:
                case OPCode.CallSuper:
                case OPCode.CallSuperVoid:
                case OPCode.ConstructProp:
                values.Add(Read7BitEncodedInt());
                values.Add(Read7BitEncodedInt());
                break;

                case OPCode.Debug:
                values.Add(ReadByte());
                values.Add(Read7BitEncodedInt());
                values.Add(ReadByte());
                values.Add(Read7BitEncodedInt());
                break;

                case OPCode.HasNext2:
                values.Add(ReadUInt32());
                values.Add(ReadUInt32());
                break;

                case OPCode.IfEq:
                case OPCode.IfFalse:
                case OPCode.IfGe:
                case OPCode.IfGt:
                case OPCode.IfLe:
                case OPCode.IfLt:
                case OPCode.IfNGe:
                case OPCode.IfNGt:
                case OPCode.IfNLe:
                case OPCode.IfNLt:
                case OPCode.IfNe:
                case OPCode.IfStrictEq:
                case OPCode.IfStrictNE:
                case OPCode.IfTrue:
                case OPCode.Jump:
                values.Add(ReadS24());
                break;

                case OPCode.LoopUpSwitch:
                {
                    values.Add(ReadS24());

                    int caseCount = (Read7BitEncodedInt() + 1);
                    values.Add(caseCount - 1);

                    for (int i = 0; i < caseCount; i++)
                        values.Add(ReadS24());

                    break;
                }

                case OPCode.PushByte:
                values.Add(ReadByte());
                break;
            }
            return values.ToArray();
        }

        /// <summary>
        /// Reads a 32-bit signed integer in compressed format.
        /// </summary>
        /// <returns>A 32-bit signed integer in compressed format.</returns>
        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        /// <summary>
        /// Reads a null-terminated character string from the current stream.
        /// </summary>
        /// <returns>The string being read.</returns>
        public string ReadNullTerminatedString()
        {
            char currentChar = '\0';
            string result = string.Empty;

            while ((currentChar = ReadChar()) != '\0')
                result += currentChar;

            return result;
        }
        /// <summary>
        /// Reads a string with the specified length.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <returns>The string being read.</returns>
        public string ReadString(int length)
        {
            return new string(ReadChars(length));
        }

        public byte[] ToArray()
        {
            return _flashStream.ToArray();
        }
        public void ResetBuffer(byte[] buffer)
        {
            _flashStream.SetLength(buffer.Length);
            Position = 0;

            BaseStream.Write(buffer, 0, buffer.Length);
            Position = 0;
        }
    }
}