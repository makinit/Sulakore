using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("{ObjName}, MultinameType: {MultinameType}")]
    public class RTQName : IMultiname
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public RTQName(ABCFile abc, FlashReader reader)
            : this(abc, reader, ConstantType.RTQName)
        { }
        public RTQName(ABCFile abc, ConstantType multinameType)
        {
            ABC = abc;
            MultinameType = multinameType;
        }
        public RTQName(ABCFile abc, FlashReader reader, ConstantType multinameType)
            : this(abc, multinameType)
        {
            ObjNameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var rtqName = new FlashWriter())
            {
                rtqName.Write7BitEncodedInt(ObjNameIndex);
                return rtqName.ToArray();
            }
        }
    }
}