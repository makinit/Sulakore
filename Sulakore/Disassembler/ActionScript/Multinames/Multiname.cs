using System;
using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("{ObjName}, MultinameType: {MultinameType}")]
    public class Multiname : IMultiname
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public ASNamespaceSet NamespaceSet
        {
            get { return ABC.Constants.NamespaceSets[NamespaceSetIndex]; }
        }
        public int NamespaceSetIndex { get; set; }

        public Multiname(ABCFile abc, FlashReader reader)
            : this(abc, reader, ConstantType.Multiname)
        { }
        public Multiname(ABCFile abc, ConstantType multinameType)
        {
            if (multinameType == ConstantType.Multiname ||
                multinameType == ConstantType.MultinameA)
            {
                ABC = abc;
                MultinameType = multinameType;
            }
            else throw new Exception($"Invalid {nameof(Multiname)} type: " + multinameType);
        }
        public Multiname(ABCFile abc, FlashReader reader, ConstantType multinameType)
            : this(abc, multinameType)
        {
            ObjNameIndex = reader.Read7BitEncodedInt();
            NamespaceSetIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var multiname = new FlashWriter())
            {
                multiname.Write7BitEncodedInt(ObjNameIndex);
                multiname.Write7BitEncodedInt(NamespaceSetIndex);
                return multiname.ToArray();
            }
        }
    }
}