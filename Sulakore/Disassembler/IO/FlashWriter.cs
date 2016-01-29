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
        private readonly MemoryStream _flashStream;

        protected int BitPosition { get; set; }
        protected byte BitContainer { get; set; }

        public int Position
        {
            get { return (int)BaseStream.Position; }
            set { BaseStream.Position = value; }
        }
        public int Length => (int)BaseStream.Length;

        public FlashWriter()
            : this(0)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashWriter"/> class based on the specified byte array.
        /// </summary>
        /// <param name="data">The initial array of unsigned bytes from which to create the stream.</param>
        public FlashWriter(byte[] data)
            : this(data.Length)
        {
            Write(data, 0, data.Length);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashWriter"/> class with the specified byte array capacity.
        /// </summary>
        /// <param name="capacity">The initial size of the internal array in bytes.</param>
        public FlashWriter(int capacity)
            : base(new MemoryStream(capacity))
        {
            _flashStream = (MemoryStream)BaseStream;
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
        public void WriteOP(OPCode op, params object[] values)
        {
            Write((byte)op);
            if (values.Length == 0) return;

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
                Write7BitEncodedInt((int)values[0]);
                break;

                case OPCode.CallMethod:
                case OPCode.CallProperty:
                case OPCode.CallPropLex:
                case OPCode.CallPropVoid:
                case OPCode.CallStatic:
                case OPCode.CallSuper:
                case OPCode.CallSuperVoid:
                case OPCode.ConstructProp:
                Write7BitEncodedInt((int)values[0]);
                Write7BitEncodedInt((int)values[1]);
                break;

                case OPCode.Debug:
                Write((byte)values[0]);
                Write7BitEncodedInt((int)values[1]);
                Write((byte)values[2]);
                Write7BitEncodedInt((int)values[3]);
                break;

                case OPCode.HasNext2:
                Write((uint)values[0]);
                Write((uint)values[1]);
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
                WriteS24((int)values[0]);
                break;

                case OPCode.LoopUpSwitch:
                {
                    WriteS24((int)values[0]);
                    Write7BitEncodedInt((int)values[1]);
                    for (int i = 2; i < values.Length; i++)
                    {
                        var value = (int)values[i];
                        WriteS24(value);
                    }
                    break;
                }

                case OPCode.PushByte:
                Write((byte)values[0]);
                break;
            }
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
        public byte[] ToArray() => _flashStream.ToArray();
    }
}