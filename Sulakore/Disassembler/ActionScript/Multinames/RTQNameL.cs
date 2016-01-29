using System;
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
            if (multinameType == ConstantType.RTQNameL ||
                multinameType == ConstantType.RTQNameLA)
            {
                ABC = abc;
                MultinameType = multinameType;
            }
            else throw new Exception($"Invalid {nameof(RTQNameL)} type: " + multinameType);
        }

        public byte[] ToByteArray()
        {
            return new byte[0];
        }
    }
}