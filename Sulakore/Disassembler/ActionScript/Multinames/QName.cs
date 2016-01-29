using System;
using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("{ObjName}, Namespace: {Namespace?.ObjName}, MultinameType: {MultinameType}")]
    public class QName : IMultiname
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public ASNamespace Namespace
        {
            get { return ABC.Constants.Namespaces[NamespaceIndex]; }
        }
        public int NamespaceIndex { get; set; }

        public QName(ABCFile abc, FlashReader reader)
            : this(abc, reader, ConstantType.QName)
        { }
        public QName(ABCFile abc, ConstantType multinameType)
        {
            if (multinameType == ConstantType.QName ||
                multinameType == ConstantType.QNameA)
            {
                ABC = abc;
                MultinameType = multinameType;
            }
            else throw new Exception($"Invalid {nameof(QName)} type: " + multinameType);
        }
        public QName(ABCFile abc, FlashReader reader, ConstantType multinameType)
            : this(abc, multinameType)
        {
            NamespaceIndex = reader.Read7BitEncodedInt();
            ObjNameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var qName = new FlashWriter())
            {
                qName.Write7BitEncodedInt(NamespaceIndex);
                qName.Write7BitEncodedInt(ObjNameIndex);
                return qName.ToArray();
            }
        }
    }
}