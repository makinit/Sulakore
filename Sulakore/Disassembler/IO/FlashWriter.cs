using System.IO;
using System.Text;
using Sulakore.Disassembler.ActionScript;

namespace Sulakore.Disassembler.IO
{
    /// <summary>
    /// Represnts a writeable Shockwave Flash(SWF) file stream.
    /// </summary>
    public class FlashWriter : BinaryWriter
    {
        private readonly MemoryStream _flashMemory;

        protected int BitPosition { get; set; }
        protected byte BitContainer { get; set; }

        public int Position
        {
            get { return (int)BaseStream.Position; }
            set { BaseStream.Position = value; }
        }
        public int Length => (int)BaseStream.Length;

        public FlashWriter() :
            this(0)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashWriter"/> class based on the specified byte array.
        /// </summary>
        /// <param name="data">The initial array of unsigned bytes from which to create the stream.</param>
        public FlashWriter(byte[] data) :
            this(data.Length)
        {
            _flashMemory.Write(data, 0, data.Length);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashWriter"/> class with the specified byte array capacity.
        /// </summary>
        /// <param name="capacity">The initial size of the internal array in bytes.</param>
        public FlashWriter(int capacity) :
            base(new MemoryStream(capacity))
        {
            _flashMemory = (MemoryStream)BaseStream;
        }

        public void WriteS24(int value)
        {
            var byteValue = (byte)(value & 0xff);
            Write(byteValue);

            value >>= 8;

            byteValue = (byte)(value & 0xff);
            Write(byteValue);

            value >>= 8;

            byteValue = (byte)(value & 0xff);
            Write(byteValue);
        }
        public void WriteSB(int bitCount, int value)
        {
            WriteUB(bitCount, value);
        }
        public void WriteUB(int bitCount, int value)
        {
            for (int bit = 0; bit < bitCount; bit++)
            {
                int nb = (value >> (bitCount - 1 - bit)) & 1;
                BitContainer += (byte)(nb * (1 << (7 - BitPosition)));

                if (++BitPosition == 8)
                {
                    BitPosition = 0;
                    Write(BitContainer);
                    BitContainer = 0;
                }
            }
        }

        public virtual void WriteOP(OPCode op)
        {
            Write((byte)op);
        }

        /// <summary>
        /// Writes specified string without any other extra information using <see cref="UTF8Encoding"/>.
        /// </summary>
        /// <param name="value">The string value to write to the stream.</param>
        public void WriteUTF8SimpleString(string value)
        {
            base.Write(Encoding.UTF8.GetBytes(value));
        }
        /// <summary>
        /// Writes a 32-bit signed integer in a compressed format.
        /// </summary>
        /// <param name="value">The 32-bit signed integer to be written.</param>
        public new void Write7BitEncodedInt(int value)
        {
            base.Write7BitEncodedInt(value);
        }
        /// <summary>
        /// Writes a null-terminated string to the stream using <see cref="UTF8Encoding"/>.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteNullTerminatedString(string value)
        {
            Write(Encoding.UTF8.GetBytes(value + "\0"));
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="MemoryStream.Position"/> property.
        /// </summary>
        public byte[] ToArray() => _flashMemory.ToArray();
    }
}