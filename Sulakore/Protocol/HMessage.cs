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
using System.Text;
using System.Collections.Generic;

using Sulakore.Protocol.Encoders;

namespace Sulakore.Protocol
{
    public class HMessage : HPacketBase
    {
        private readonly List<byte> _body;

        private byte[] _toBytesCache;
        private string _toStringCache;
        private bool _beganConstructing;

        private ushort _header;
        /// <summary>
        /// Gets or sets the header of the <see cref="HMessage"/>.
        /// </summary>
        public override ushort Header
        {
            get { return _header; }
            set
            {
                if (!IsCorrupted || _header != value)
                {
                    _header = value;
                    ResetCache();
                }
            }
        }

        /// <summary>
        /// Gets or sets the position that determines where to begin the next read/write operation in the <see cref="HMessage"/>.
        /// </summary>
        public override int Position { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="HDestination"/> for the <see cref="HMessage"/>.
        /// </summary>
        public override HDestination Destination { get; set; }

        /// <summary>
        /// Gets a value that determines whether the <see cref="HMessage"/> is readable/writable.
        /// </summary>
        public override bool IsCorrupted { get; }
        /// <summary>
        /// Gets the length of the <see cref="HMessage"/>.
        /// </summary>
        public override int Length { get; protected set; }
        /// <summary>
        /// Gets the block of data considered as the body of the packet that excludes the header.
        /// </summary>
        public override byte[] Body { get; protected set; }

        private readonly List<object> _read;
        /// <summary>
        /// Gets a <see cref="IReadOnlyList{T}"/> of type <see cref="object"/> containing read values from the <see cref="HMessage"/>.
        /// </summary>
        public IReadOnlyList<object> ValuesRead => _read;

        private readonly List<object> _written;
        /// <summary>
        /// Gets a <see cref="IReadOnlyList{T}"/> of type <see cref="object"/> containing the written values of the <see cref="HMessage"/>.
        /// </summary>
        public IReadOnlyList<object> ValuesWritten => _written;

        private HMessage()
        {
            _body = new List<byte>();
            _read = new List<object>();
            _written = new List<object>();
        }
        public HMessage(byte[] data)
            : this(data, HDestination.Client)
        { }
        public HMessage(string packet)
            : this(ToBytes(packet), HDestination.Client)
        { }
        public HMessage(string packet, HDestination destination)
            : this(ToBytes(packet), destination)
        { }
        public HMessage(byte[] data, HDestination destination)
            : this()
        {
            Destination = destination;
            IsCorrupted = (BigEndian.ToSI32(data) != data.Length - 4);

            if (!IsCorrupted)
            {
                Header = BigEndian.ToUI16(data, 4);

                _body.AddRange(data);
                _body.RemoveRange(0, 6);

                Reconstruct();
            }
            else
            {
                Length = data.Length;
                _toBytesCache = data;
            }
        }
        public HMessage(ushort header, params object[] chunks)
            : this(Construct(header, chunks), HDestination.Client)
        {
            _beganConstructing = true;
            AddToWritten(chunks);
        }

        public void Write(params object[] chunks)
        {
            AddToWritten(chunks);
            byte[] constructed = Encode(chunks);

            _body.AddRange(constructed);
            Reconstruct();
        }
        public override int ReadInteger(ref int index)
        {
            if (index + 4 > Body.Length)
                throw new Exception($"Not enough data at position:{index} to read 4 bytes(32-Bit Integer).");

            int value = BigEndian.ToSI32(Body, index);
            index += 4;

            AddToRead(value);
            return value;
        }
        public override ushort ReadShort(ref int index)
        {
            if (index + 2 > Body.Length)
                throw new Exception($"Not enough data at position:{index} to read 2 bytes(16-Bit Unsigned Integer).");

            ushort value = BigEndian.ToUI16(Body, index);
            index += 2;

            AddToRead(value);
            return value;
        }
        public override bool ReadBoolean(ref int index)
        {
            if (index + 1 > Body.Length)
                throw new Exception($"Not enough data at position:{index} to read a byte(Boolean - True|False).");

            bool value = (Body[index++] == 1);

            AddToRead(value);
            return value;
        }
        public override string ReadString(ref int index)
        {
            ushort length = ReadShort(ref index);

            if (index + length > Body.Length)
                throw new Exception($"Not enough data at position:{index} to read a UTF-8 encoded string with the length of {length} bytes.");

            byte[] stringData = ReadBytes(length, ref index);
            string value = Encoding.UTF8.GetString(stringData);

            AddToRead(value);
            return value;
        }
        public override byte[] ReadBytes(int length, ref int index)
        {
            if (length + index > Body.Length)
                throw new Exception($"Not enough data at position:{index} to read {length} bytes.");

            byte[] value = new byte[length];
            Buffer.BlockCopy(Body, index, value, 0, length);
            index += length;

            AddToRead(value);
            return value;
        }

        public override void Remove<T>(int index)
        {
            int valueSize = 0;
            switch (Type.GetTypeCode(typeof(T)))
            {
                default: return;

                case TypeCode.Int32: valueSize = 4; break;
                case TypeCode.UInt16: valueSize = 2; break;
                case TypeCode.Boolean: valueSize = 1; break;
                case TypeCode.String:
                    {
                        int stringLength = BigEndian.ToUI16(Body, index);
                        valueSize = (2 + stringLength);
                        break;
                    }
            }

            _body.RemoveRange(index, valueSize);
            Reconstruct();
        }
        public override bool CanRead<T>(int index)
        {
            int bytesLeft = (Body.Length - index), bytesNeeded = -1;
            if (bytesLeft < 1) return false;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: bytesNeeded = 4; break;
                case TypeCode.UInt16: bytesNeeded = 2; break;
                case TypeCode.Boolean: bytesNeeded = 1; break;
                case TypeCode.String:
                    {
                        if (bytesLeft > 2)
                        {
                            int stringLength = BigEndian.ToUI16(Body, index);
                            bytesNeeded = (2 + stringLength);
                        }
                        break;
                    }
            }
            return bytesLeft >= bytesNeeded && bytesNeeded != -1;
        }
        public override void Replace<T>(int index, object chunk)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: _body.RemoveRange(index, 4); break;
                case TypeCode.UInt16: _body.RemoveRange(index, 2); break;
                case TypeCode.Boolean: _body.RemoveAt(index); break;
                case TypeCode.String:
                    {
                        int stringLength = BigEndian.ToUI16(Body, index);
                        _body.RemoveRange(index, 2 + stringLength);
                        break;
                    }
            }

            _body.InsertRange(index, Encode(chunk));
            Reconstruct();
        }

        public void ClearWritten()
        {
            if (!_beganConstructing)
                throw new Exception("This method cannot be called on a packet that you did not begin constructing.");

            if (_written.Count < 1)
                return;

            _body.Clear();
            _written.Clear();

            Reconstruct();
        }
        public void RemoveWritten(int index)
        {
            if (!_beganConstructing)
                throw new Exception("This method cannot be called on a packet that you did not begin constructing.");

            if (index < 0 || index >= _written.Count)
                return;

            _written.RemoveAt(index);

            _body.Clear();
            if (_written.Count > 0)
                _body.AddRange(Encode(_written.ToArray()));

            Reconstruct();
        }
        public void ReplaceWritten(int index, object chunk)
        {
            if (!_beganConstructing)
                throw new Exception("This method cannot be called on a packet that you did not begin constructing.");

            if (index < 0 || index >= _written.Count)
                return;

            _written[index] = chunk;

            _body.Clear();
            _body.AddRange(Encode(_written.ToArray()));
            Reconstruct();
        }
        public void MoveWritten(int index, int jump, bool toRight)
        {
            if (!_beganConstructing)
                throw new Exception("This method cannot be called on a packet that you did not begin constructing.");

            if (jump < 1) return;
            int newIndex = (toRight ? index + jump : index - jump);
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= _written.Count) newIndex = _written.Count - 1;

            object chunk = _written[index];
            _written.Remove(chunk);
            _written.Insert(newIndex, chunk);

            _body.Clear();
            _body.AddRange(Encode(_written.ToArray()));
            Reconstruct();
        }

        private void AddToRead(params object[] chunks)
        {
            _read.AddRange(chunks);
        }
        private void AddToWritten(params object[] chunks)
        {
            _written.AddRange(chunks);
        }

        private void ResetCache()
        {
            _toBytesCache = null;
            _toStringCache = null;
        }
        private void Reconstruct()
        {
            ResetCache();

            Length = _body.Count + 2;
            Body = new byte[_body.Count];

            Buffer.BlockCopy(_body.ToArray(), 0, Body, 0, Body.Length);
        }

        public byte[] ToBytes()
        {
            return _toBytesCache ??
                (_toBytesCache = Construct(Header, Body));
        }
        public override string ToString()
        {
            return _toStringCache ??
                (_toStringCache = ToString(ToBytes()));
        }

        public static byte[] ToBytes(string packet)
        {
            for (int i = 0; i <= 13; i++)
                packet = packet.Replace("[" + i + "]", ((char)i).ToString());

            string byteValue = string.Empty;
            while (!string.IsNullOrWhiteSpace(
                byteValue = packet.GetChild("{b:", '}').ToLower()))
            {
                char byteChar = byteValue[0];
                byte value = (byte)(byteChar == 't' || byteChar == '1' ? 1 : 0);
                if (byteValue[0] != 'f' && value != 1 &&
                    !byte.TryParse(byteValue, out value))
                {
                    break;
                }

                string byteParam = $"{{b:{byteValue}}}";
                packet = packet.Replace(byteParam, ((char)value).ToString());
            }

            string ushortValue = string.Empty;
            while (!string.IsNullOrWhiteSpace(
                ushortValue = packet.GetChild("{u:", '}')))
            {
                ushort value = 0;
                if (!ushort.TryParse(ushortValue, out value)) break;

                byte[] ushortData = Encode(value);
                string ushortParam = $"{{u:{ushortValue}}}";

                packet = packet.Replace(ushortParam,
                    Encoding.Default.GetString(ushortData));
            }

            string intValue = string.Empty;
            while (!string.IsNullOrWhiteSpace(
                intValue = packet.GetChild("{i:", '}')))
            {
                int value = 0;
                if (!int.TryParse(intValue, out value)) break;

                byte[] intData = Encode(value);
                string intParam = $"{{i:{intValue}}}";

                packet = packet.Replace(intParam,
                    Encoding.Default.GetString(intData));
            }

            string stringValue = string.Empty;
            while (!string.IsNullOrWhiteSpace(
                stringValue = packet.GetChild("{s:", '}')))
            {
                byte[] stringData = Encode(stringValue);
                string stringParam = $"{{s:{stringValue}}}";

                packet = packet.Replace(stringParam,
                    Encoding.Default.GetString(stringData));
            }

            if (packet.StartsWith("{l}"))
            {
                byte[] lengthData = Encode(packet.Length - 3);
                packet = Encoding.Default.GetString(lengthData) + packet.Substring(3);
            }

            return Encoding.Default.GetBytes(packet);
        }
        public static string ToString(byte[] packet)
        {
            string result = Encoding.Default.GetString(packet);

            for (int i = 0; i <= 13; i++)
                result = result.Replace(((char)i).ToString(), "[" + i + "]");

            return result;
        }
        public static byte[] Encode(params object[] chunks)
        {
            if (chunks.Length < 1)
                return new byte[0];

            var buffer = new List<byte>();
            for (int i = 0; i < chunks.Length; i++)
            {
                object chunk = chunks[i];
                if (chunk == null)
                    throw new NullReferenceException("chunk");

                switch (Type.GetTypeCode(chunk.GetType()))
                {
                    case TypeCode.Byte: buffer.Add((byte)chunk); break;
                    case TypeCode.Boolean: buffer.Add(Convert.ToByte((bool)chunk)); break;
                    case TypeCode.Int32: buffer.AddRange(BigEndian.FromSI32((int)chunk)); break;
                    case TypeCode.UInt16: buffer.AddRange(BigEndian.FromUI16((ushort)chunk)); break;

                    default:
                    case TypeCode.String:
                        {
                            byte[] data = chunk as byte[];
                            if (data == null)
                            {
                                string value = chunk.ToString()
                                    .Replace("\\a", "\a").Replace("\\b", "\b")
                                    .Replace("\\f", "\f").Replace("\\n", "\n")
                                    .Replace("\\r", "\r").Replace("\\t", "\t")
                                    .Replace("\\v", "\v").Replace("\\0", "\0");

                                byte[] stringData = Encoding.UTF8.GetBytes(value);

                                data = new byte[2 + Encoding.UTF8.GetByteCount(value)];
                                Buffer.BlockCopy(BigEndian.FromUI16((ushort)(data.Length - 2)), 0, data, 0, 2);
                                Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, data, 2, data.Length - 2);
                            }
                            buffer.AddRange(data);
                            break;
                        }
                }
            }
            return buffer.ToArray();
        }
        public static byte[] Construct(ushort header, params object[] chunks)
        {
            byte[] body = (chunks != null && chunks.Length > 0) ?
                Encode(chunks) : new byte[0];

            byte[] data = new byte[6 + body.Length];
            Buffer.BlockCopy(BigEndian.FromSI32(body.Length + 2), 0, data, 0, 4);
            Buffer.BlockCopy(BigEndian.FromUI16(header), 0, data, 4, 2);
            Buffer.BlockCopy(body, 0, data, 6, body.Length);
            return data;
        }
    }
}