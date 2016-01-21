using System.Diagnostics;

using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("MultinameType: {MultinameType}")]
    public class RTQNameL : IMultiname
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType { get; }

        public RTQNameL(ABCFile abc)
            : this(abc, ConstantType.RTQNameL)
        { }
        public RTQNameL(ABCFile abc, ConstantType multinameType)
        {
            ABC = abc;
            MultinameType = multinameType;
        }

        public byte[] ToByteArray()
        {
            return new byte[0];
        }
    }
}