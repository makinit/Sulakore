using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Constants
{
    [DebuggerDisplay("{ObjName}, NamespaceType: {NamespaceType}")]
    public class ASNamespace : IConstant
    {
        public ABCFile ABC { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public ConstantType NamespaceType { get; set; }
        public int ConstantIndex { get; internal set; }

        public ASNamespace(ABCFile abc)
        {
            ABC = abc;
        }
        public ASNamespace(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            NamespaceType = (ConstantType)reader.ReadByte();
            ObjNameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var asNamespace = new FlashWriter())
            {
                asNamespace.Write((byte)NamespaceType);
                asNamespace.Write7BitEncodedInt(ObjNameIndex);
                return asNamespace.ToArray();
            }
        }
    }
}