using System.IO;

using FlashInspect.ActionScript;

namespace FlashInspect.IO
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

        public OPCode ReadOP()
        {
            return (OPCode)ReadByte();
        }

        /// <summary>
        /// Reads in a 32-bit signed integer in compressed format.
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