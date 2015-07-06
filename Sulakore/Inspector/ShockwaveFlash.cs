/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Sulakore.Inspector.Tags;

namespace Sulakore.Inspector
{
    public class ShockwaveFlash
    {
        private byte[] _buffer;
        private int _bitPosition;
        private byte _currentByte;

        private static readonly float[] _powers;
        private static readonly uint[] _bitValues;

        private int _position;
        public int Position { get { return _position; } }

        public int Version { get; private set; }
        public uint FileLength { get; private set; }
        public string Signature { get; private set; }

        public int FrameRate { get; private set; }
        public Rect FrameSize { get; private set; }
        public int FrameCount { get; private set; }

        private readonly IList<IFlashTag> _tags;
        public ReadOnlyCollection<IFlashTag> Tags { get; private set; }

        static ShockwaveFlash()
        {
            _bitValues = new uint[32];
            for (byte power = 0; power < 32; power++)
                _bitValues[power] = (uint)(1 << power);

            _powers = new float[32];
            for (byte power = 0; power < 32; power++)
                _powers[power] = (float)Math.Pow(2, power - 16);
        }
        public ShockwaveFlash(byte[] data)
        {
            if (data.Length < 8)
                throw new Exception("Not enough data to parse file header.");

            _buffer = data;
            _tags = new List<IFlashTag>();
            Tags = new ReadOnlyCollection<IFlashTag>(_tags);

            Signature = Encoding.UTF8.GetString(ReadUI8Block(3));
            Version = ReadUI8();
            FileLength = ReadUI32();

            switch (Signature)
            {
                case "CWS":
                {
                    Decompress();
                    break;
                }
                case "FWS": break;
                case "ZWS": throw new Exception("Unsupported signature: ZWS(LZMA)");
                default: throw new Exception("Unknown signature");
            }

            FrameSize = new Rect(this);
            FrameRate = (ReadUI16() >> 8);
            FrameCount = ReadUI16();
            ExtractTags();
        }
        public ShockwaveFlash(string path)
            : this(File.ReadAllBytes(path))
        { }

        private void Decompress()
        {
            using (var decompressedStream = new MemoryStream((int)FileLength))
            using (var bufferStream = new MemoryStream(_buffer, 10, _buffer.Length - 10))
            using (var decompresser = new DeflateStream(bufferStream, CompressionMode.Decompress))
            {
                decompressedStream.Write(_buffer, 0, 8);
                decompresser.CopyTo(decompressedStream);

                _buffer = decompressedStream.ToArray();
                _buffer[0] = (byte)'F';
            }
        }
        private void ExtractTags()
        {
            RecordHeader header;
            int expectedPosition;
            IFlashTag currentTag;
            do
            {
                header = new RecordHeader(this);
                expectedPosition = (_position + header.Length);

                switch (header.TagType)
                {
                    default: currentTag = new UnsupportedTag(this, header); break;

                    // TODO: Add param to determine whether to process the certain tags, some just take too long.

                    //case TagType.DebugId: currentTag = new DebugIdTag(this, header); break;

                    //case TagType.DoABC:
                    //case TagType.DoABC2: currentTag = new DoAbcTag(this, header); break;

                    //case TagType.Metadata: currentTag = new MetadataTag(this, header); break;
                    //case TagType.FileAttributes: currentTag = new FileAttributesTag(this, header); break;
                    //case TagType.FrameLabel: currentTag = new FrameLabelTag(this, header); break;

                    //case TagType.ProductInfo: currentTag = new ProductInfoTag(this, header); break;

                    //case TagType.ShowFrame: currentTag = new ShowFrameTag(this, header); break;
                    //case TagType.SymbolClass: currentTag = new SymbolClassTag(this, header); break;
                    //case TagType.ScriptLimits: currentTag = new ScriptLimitsTag(this, header); break;
                    //case TagType.SetBackgroundColor: currentTag = new SetBackgroundColorTag(this, header); break;

                    //case TagType.End: currentTag = new EndTag(this, header); break;
                    //case TagType.EnableDebugger2: currentTag = new EnableDebugger2Tag(this, header); break;

                    case TagType.DefineBinaryData: currentTag = new DefineBinaryDataTag(this, header); break;

                    case TagType.DoInitAction:
                    case TagType.DefineBitsLossless2:
                    case TagType.DefineBitsJPEG3: currentTag = new UnsupportedTag(this, header, true); break;
                }
                _tags.Add(currentTag);

                if (_position != expectedPosition)
                    throw new Exception(string.Format("{0} was not correctly parsed.", header.TagType));
            }
            while (header.TagType != TagType.End);
        }

        #region Read/Write Methods
        private bool ReadBit()
        {
            if (_bitPosition > 7)
            {
                _currentByte = ReadUI8();
                _bitPosition = 0;
            }
            return ((_currentByte & _bitValues[(7 - _bitPosition++)]) != 0);
        }
        public int ReadSB(int bits)
        {
            if (bits < 1) return 0;

            int result = 0;
            if (ReadBit())
                result -= (int)_bitValues[bits - 1];

            for (int index = bits - 2; index > -1; index--)
                if (ReadBit())
                    result |= (int)_bitValues[index];

            return result;
        }
        public uint ReadUB(int bits)
        {
            if (bits < 1) return 0;

            uint result = 0;
            for (int index = bits - 1; index > -1; index--)
                if (ReadBit())
                    result |= _bitValues[index];

            return result;
        }
        public float ReadFB(int bits)
        {
            if (bits < 1) return 0;

            float result = 0;
            if (ReadBit())
                result -= _powers[bits - 1];

            for (int index = bits - 1; index > 0; index--)
                if (ReadBit())
                    result += _powers[index - 1];

            return result;
        }

        public sbyte ReadSI8()
        {
            return ReadSI8(ref _position);
        }
        public sbyte ReadSI8(int index)
        {
            return ReadSI8(ref index);
        }
        public sbyte ReadSI8(ref int index)
        {
            return (sbyte)ReadUI8(ref index);
        }

        public byte ReadUI8()
        {
            return ReadUI8(ref _position);
        }
        public byte ReadUI8(int index)
        {
            return ReadUI8(ref index);
        }
        public byte ReadUI8(ref int index)
        {
            _bitPosition = 8;
            return _buffer[index++];
        }

        public byte[] ReadUI8Block(int length)
        {
            return ReadUI8Block(length, ref _position);
        }
        public byte[] ReadUI8Block(int length, int index)
        {
            return ReadUI8Block(length, ref index);
        }
        public byte[] ReadUI8Block(int length, ref int index)
        {
            byte[] buffer = new byte[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadUI8(ref index);

            return buffer;
        }

        public short ReadSI16() { return ReadSI16(ref _position); }
        public short ReadSI16(int index)
        {
            return ReadSI16(ref index);
        }
        public short ReadSI16(ref int index)
        {
            return (short)ReadUI16(ref index);
        }

        public ushort ReadUI16()
        {
            return ReadUI16(ref _position);
        }
        public ushort ReadUI16(int index)
        {
            return ReadUI16(ref index);
        }
        public ushort ReadUI16(ref int index)
        {
            ushort value = 0;
            value |= (ushort)ReadUI8(ref index);
            value |= (ushort)(ReadUI8(ref index) << 8);
            return value;
        }

        public int ReadSI32() { return ReadSI32(ref _position); }
        public int ReadSI32(int index)
        {
            return ReadSI32(ref index);
        }
        public int ReadSI32(ref int index)
        {
            return (int)ReadUI32(ref index);
        }

        public uint ReadUI32()
        {
            return ReadUI32(ref _position);
        }
        public uint ReadUI32(int index)
        {
            return ReadUI32(ref index);
        }
        public uint ReadUI32(ref int index)
        {
            uint value = 0;
            value |= (uint)ReadUI8(ref index);
            value |= (uint)(ReadUI8(ref index) << 8);
            value |= (uint)(ReadUI8(ref index) << 16);
            value |= (uint)(ReadUI8(ref index) << 24);
            return value;
        }

        public long ReadSI64()
        {
            return ReadSI64(ref _position);
        }
        public long ReadSI64(int index)
        {
            return ReadSI64(ref index);
        }
        public long ReadSI64(ref int index)
        {
            return (long)ReadUI64(ref index);
        }
        public ulong ReadUI64()
        {
            return ReadUI64(ref _position);
        }
        public ulong ReadUI64(int index)
        {
            return ReadUI64(ref index);
        }
        public ulong ReadUI64(ref int index)
        {
            ulong value = BitConverter.ToUInt64(_buffer, index);

            index += (_bitPosition = 8);
            return value;
        }

        public bool ReadBool()
        {
            return (ReadUB(1) == 1);
        }
        public string ReadString()
        {
            byte[] chop = new byte[_buffer.Length - Position];
            Buffer.BlockCopy(_buffer, Position, chop, 0, chop.Length);
            byte[] outcome = chop.TakeWhile(b => b != 0).ToArray();
            _position += outcome.Length + 1;

            return Encoding.UTF8.GetString(outcome);
        }
        #endregion

        public byte[] Build()
        {
            var bodyBuffer = new List<byte>();
            bodyBuffer.AddRange(FrameSize.ToBytes());
            bodyBuffer.AddRange(BitConverter.GetBytes((ushort)(FrameRate << 8)));
            bodyBuffer.AddRange(BitConverter.GetBytes((ushort)FrameCount));

            foreach (IFlashTag tag in Tags)
                bodyBuffer.AddRange(tag.ToBytes());

            var buffer = new List<byte>(8 + bodyBuffer.Count);
            buffer.AddRange(Encoding.UTF8.GetBytes("FWS"));
            buffer.Add((byte)Version);
            buffer.AddRange(BitConverter.GetBytes(buffer.Capacity));
            buffer.AddRange(bodyBuffer);

            _buffer = buffer.ToArray();
            _position = _buffer.Length;

            return _buffer;
        }
        public byte[] ToBytes()
        {
            return _buffer;
        }
        public void Save(string path)
        {
            File.WriteAllBytes(path, _buffer);
        }

        public static byte[] ConstructTag(TagType tagType, byte[] data)
        {
            return ConstructTag(tagType, data, false);
        }
        public static byte[] ConstructTag(TagType tagType, byte[] data, bool asLong)
        {
            var header = ((uint)tagType) << 6;
            bool isLong = (asLong || data.Length >= 0x3f);
            header |= (isLong ? 0x3f : (uint)data.Length);

            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((ushort)header));
            if (isLong) buffer.AddRange(BitConverter.GetBytes(data.Length));

            buffer.AddRange(data);

            return buffer.ToArray();
        }
    }
}