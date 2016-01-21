using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("MultinameType: {MultinameType}")]
    public class MultinameL : IMultiname
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType { get; }

        public ASNamespaceSet NamespaceSet
        {
            get { return ABC.Constants.NamespaceSets[NamespaceSetIndex]; }
        }
        public int NamespaceSetIndex { get; set; }

        public MultinameL(ABCFile abc, FlashReader reader)
            : this(abc, reader, ConstantType.MultinameL)
        { }
        public MultinameL(ABCFile abc, ConstantType multinameType)
        {
            ABC = abc;
            MultinameType = multinameType;
        }
        public MultinameL(ABCFile abc, FlashReader reader, ConstantType multinameType)
            : this(abc, multinameType)
        {
            NamespaceSetIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var multinameL = new FlashWriter())
            {
                multinameL.Write7BitEncodedInt(NamespaceSetIndex);
                return multinameL.ToArray();
            }
        }
    }
}