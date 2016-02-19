using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sulakore.Protocol
{
    [DebuggerDisplay("Header: {Header}, Length: {Length} | {ToString()}")]
    public sealed class HMessage
    {
        private readonly List<byte> _body;

        private string _toStringCache;
        private byte[] _toBytesCache, _bodyBuffer;

        private int _position;
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private ushort _header;
        public ushort Header
        {
            get { return _header; }
            set
            {
                if (!IsCorrupted && _header != value)
                {
                    _header = value;
                    ResetCache();
                }
            }
        }

        public int Length => (_body.Count + (!IsCorrupted ? 2 : 0));
        public int Readable => (_body.Count - Position);

        public bool IsCorrupted { get; }
        public HDestination Destination { get; set; }

        private readonly List<object> _read;
        public IReadOnlyList<object> ValuesRead => _read;

        private readonly List<object> _written;
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
        public HMessage(string data)
            : this(ToBytes(data), HDestination.Client)
        { }
        public HMessage(string data, HDestination destination)
            : this(ToBytes(data), destination)
        { }
        public HMessage(byte[] data, HDestination destination)
            : this()
        {
            Destination = destination;
            IsCorrupted = (data.Length < 6 ||
                (BigEndian.ToInt32(data, 0) != data.Length - 4));

            if (!IsCorrupted)
            {
                Header = BigEndian.ToUInt16(data, 4);

                _bodyBuffer = new byte[data.Length - 6];
                Buffer.BlockCopy(data, 6, _bodyBuffer, 0, data.Length - 6);
            }
            else _bodyBuffer = data;

            _body.AddRange(_bodyBuffer);
        }
        public HMessage(ushort header, params object[] values)
            : this(Construct(header, values), HDestination.Client)
        {
            _written.AddRange(values);
        }

        #region Read Methods
        public int ReadInteger()
        {
            return ReadInteger(ref _position);
        }
        public int ReadInteger(int position)
        {
            return ReadInteger(ref position);
        }
        public int ReadInteger(ref int position)
        {
            int value = BigEndian.ToInt32(_bodyBuffer, position);
            position += BigEndian.GetSize(value);

            _read.Add(value);
            return value;
        }

        public ushort ReadShort()
        {
            return ReadShort(ref _position);
        }
        public ushort ReadShort(int position)
        {
            return ReadShort(ref position);
        }
        public ushort ReadShort(ref int position)
        {
            ushort value = BigEndian.ToUInt16(_bodyBuffer, position);
            position += BigEndian.GetSize(value);

            _read.Add(value);
            return value;
        }

        public bool ReadBoolean()
        {
            return ReadBoolean(ref _position);
        }
        public bool ReadBoolean(int position)
        {
            return ReadBoolean(ref position);
        }
        public bool ReadBoolean(ref int position)
        {
            bool value = BigEndian.ToBoolean(_bodyBuffer, position);
            position += BigEndian.GetSize(value);

            _read.Add(value);
            return value;
        }

        public string ReadString()
        {
            return ReadString(ref _position);
        }
        public string ReadString(int position)
        {
            return ReadString(ref position);
        }
        public string ReadString(ref int position)
        {
            string value = BigEndian.ToString(_bodyBuffer, position);
            position += BigEndian.GetSize(value);

            _read.Add(value);
            return value;
        }

        public byte[] ReadBytes(int length)
        {
            return ReadBytes(length, ref _position);
        }
        public byte[] ReadBytes(int length, int position)
        {
            return ReadBytes(length, ref position);
        }
        public byte[] ReadBytes(int length, ref int position)
        {
            var value = new byte[length];
            Buffer.BlockCopy(_bodyBuffer, position, value, 0, length);
            position += length;

            _read.Add(value);
            return value;
        }
        #endregion
        #region Write Methods
        public void WriteInteger(int value)
        {
            WriteInteger(value, _body.Count);
        }
        public void WriteInteger(int value, int position)
        {
            byte[] encoded = BigEndian.GetBytes(value);
            WriteObject(encoded, value, position);
        }

        public void WriteShort(ushort value)
        {
            WriteShort(value, _body.Count);
        }
        public void WriteShort(ushort value, int position)
        {
            byte[] encoded = BigEndian.GetBytes(value);
            WriteObject(encoded, value, position);
        }

        public void WriteBoolean(bool value)
        {
            WriteBoolean(value, _body.Count);
        }
        public void WriteBoolean(bool value, int position)
        {
            byte[] encoded = BigEndian.GetBytes(value);
            WriteObject(encoded, value, position);
        }

        public void WriteString(string value)
        {
            WriteString(value, _body.Count);
        }
        public void WriteString(string value, int position)
        {
            byte[] encoded = BigEndian.GetBytes(value);
            WriteObject(encoded, value, position);
        }

        public void WriteBytes(byte[] value)
        {
            WriteBytes(value, _body.Count);
        }
        public void WriteBytes(byte[] value, int position)
        {
            WriteObject(value, value, position);
        }

        public void WriteObjects(params object[] values)
        {
            _written.AddRange(values);
            _body.AddRange(GetBytes(values));

            Refresh();
        }
        private void WriteObject(byte[] encoded, object value, int position)
        {
            _written.Add(value);
            _body.InsertRange(position, encoded);

            Refresh();
        }
        #endregion
        #region Remove Methods
        public void RemoveInteger()
        {
            RemoveInteger(_position);
        }
        public void RemoveInteger(int position)
        {
            RemoveBytes(4, position);
        }

        public void RemoveShort()
        {
            RemoveShort(_position);
        }
        public void RemoveShort(int position)
        {
            RemoveBytes(2, position);
        }

        public void RemoveBoolean()
        {
            RemoveBoolean(_position);
        }
        public void RemoveBoolean(int position)
        {
            RemoveBytes(1, position);
        }

        public void RemoveString()
        {
            RemoveString(_position);
        }
        public void RemoveString(int position)
        {
            int readable = (_body.Count - position);
            if (readable < 2) return;

            ushort stringLength =
                BigEndian.ToUInt16(_bodyBuffer, position);

            if (readable >= (stringLength + 2))
                RemoveBytes(stringLength + 2, position);
        }

        public void RemoveBytes(int length)
        {
            RemoveBytes(length, _position);
        }
        public void RemoveBytes(int length, int position)
        {
            _body.RemoveRange(position, length);
            Refresh();
        }
        #endregion

        public bool CanReadString()
        {
            return CanReadString(_position);
        }
        public bool CanReadString(int position)
        {
            int readable = (_body.Count - position);
            if (readable < 2) return false;

            ushort stringLength =
                BigEndian.ToUInt16(_bodyBuffer, position);

            return (readable >= (stringLength + 2));
        }

        public void ReplaceString(string value)
        {
            ReplaceString(value, _position);
        }
        public void ReplaceString(string value, int position)
        {
            RemoveString(position);
            WriteString(value, position);
        }

        private void Refresh()
        {
            ResetCache();
            _bodyBuffer = _body.ToArray();
        }
        private void ResetCache()
        {
            _toBytesCache = null;
            _toStringCache = null;
        }

        public byte[] ToBytes()
        {
            if (IsCorrupted)
                _toBytesCache = _bodyBuffer;

            return _toBytesCache ??
                (_toBytesCache = Construct(Header, _bodyBuffer));
        }
        public static byte[] ToBytes(string value)
        {
            // TODO: All of this.
            value = value.Replace("{b:}", "[0]")
                .Replace("{u:}", "[0][0]")
                .Replace("{s:}", "[0][0]")
                .Replace("{i:}", "[0][0][0][0]");

            for (int i = 0; i <= 13; i++)
            {
                value = value.Replace(
                    "[" + i + "]", ((char)i).ToString());
            }

            string objectValue = string.Empty;
            while (!string.IsNullOrWhiteSpace(
                objectValue = value.GetChild("{b:").GetParent("}")))
            {
                char byteChar = objectValue.ToLower()[0];

                byte byteValue = (byte)(byteChar == 't' ||
                    (byteChar == '1' && objectValue.Length == 1) ? 1 : 0);

                if (byteChar != 'f' && byteValue != 1 &&
                    !byte.TryParse(objectValue.ToLower(), out byteValue))
                {
                    break;
                }

                string byteParam = $"{{b:{objectValue}}}";
                value = value.Replace(byteParam, ((char)byteValue).ToString());
            }

            while (!string.IsNullOrWhiteSpace(
                objectValue = value.GetChild("{u:").GetParent("}")))
            {
                ushort shortValue = 0;
                if (!ushort.TryParse(objectValue, out shortValue)) break;

                byte[] ushortData = BigEndian.GetBytes(shortValue);
                string ushortParam = $"{{u:{objectValue}}}";

                value = value.Replace(ushortParam,
                    Encoding.Default.GetString(ushortData));
            }

            while (!string.IsNullOrWhiteSpace(
                objectValue = value.GetChild("{i:").GetParent("}")))
            {
                int intValue = 0;
                if (!int.TryParse(objectValue, out intValue)) break;

                byte[] intData = BigEndian.GetBytes(intValue);
                string intParam = $"{{i:{objectValue}}}";

                value = value.Replace(intParam,
                    Encoding.Default.GetString(intData));
            }

            while (!string.IsNullOrWhiteSpace(
                objectValue = value.GetChild("{s:").GetParent("}")))
            {
                byte[] stringData = BigEndian.GetBytes(objectValue);
                string stringParam = $"{{s:{objectValue}}}";

                value = value.Replace(stringParam,
                    Encoding.Default.GetString(stringData));
            }

            if (value.StartsWith("{l}") && value.Length >= 5)
            {
                byte[] lengthData = BigEndian.GetBytes(value.Length - 3);
                value = Encoding.Default.GetString(lengthData) + value.Substring(3);
            }
            return Encoding.Default.GetBytes(value);
        }

        public override string ToString()
        {
            return _toStringCache ??
                (_toStringCache = ToString(ToBytes()));
        }
        public static string ToString(byte[] value)
        {
            string result = Encoding.Default.GetString(value);
            for (int i = 0; i <= 13; i++)
            {
                result = result.Replace(
                    ((char)i).ToString(), "[" + i + "]");
            }
            return result;
        }

        public static byte[] GetBytes(params object[] values)
        {
            var buffer = new List<byte>();
            foreach (object value in values)
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Byte: buffer.Add((byte)value); break;
                    case TypeCode.Boolean: buffer.Add(Convert.ToByte((bool)value)); break;
                    case TypeCode.Int32: buffer.AddRange(BigEndian.GetBytes((int)value)); break;
                    case TypeCode.UInt16: buffer.AddRange(BigEndian.GetBytes((ushort)value)); break;

                    default:
                    case TypeCode.String:
                    {
                        byte[] data = value as byte[];
                        if (data == null)
                        {
                            string stringValue = value.ToString()
                               .Replace("\\a", "\a").Replace("\\b", "\b")
                               .Replace("\\f", "\f").Replace("\\n", "\n")
                               .Replace("\\r", "\r").Replace("\\t", "\t")
                               .Replace("\\v", "\v").Replace("\\0", "\0");

                            data = BigEndian.GetBytes(stringValue);
                        }
                        buffer.AddRange(data);
                        break;
                    }
                }
            }
            return buffer.ToArray();
        }
        public static byte[] Construct(ushort header, params object[] values)
        {
            byte[] body = GetBytes(values);
            var buffer = new byte[6 + body.Length];

            byte[] headerData = BigEndian.GetBytes(header);
            byte[] lengthData = BigEndian.GetBytes(2 + body.Length);

            Buffer.BlockCopy(lengthData, 0, buffer, 0, 4);
            Buffer.BlockCopy(headerData, 0, buffer, 4, 2);
            Buffer.BlockCopy(body, 0, buffer, 6, body.Length);
            return buffer;
        }
    }
}