using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript.Multinames
{
    public class QName : IMultiname
    {
        private readonly ASConstants _constants;

        public ConstantType MultinameType { get; }

        public string Name
        {
            get { return _constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ASNamespace Namespace
        {
            get { return _constants.Namespaces[NamespaceIndex]; }
        }
        public int NamespaceIndex { get; set; }

        public QName(ASConstants constants) :
            this(constants, 0, 0, ConstantType.QName)
        { }
        public QName(ASConstants constants, ConstantType multinameType) :
            this(constants, 0, 0, multinameType)
        { }
        public QName(ASConstants constants, int nameIndex, int namespaceIndex, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NameIndex = nameIndex;
            NamespaceIndex = namespaceIndex;
        }

        public QName(ASConstants constants, FlashReader reader) :
            this(constants, reader, ConstantType.QName)
        { }
        public QName(ASConstants constants, FlashReader reader, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NamespaceIndex = reader.Read7BitEncodedInt();
            NameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NamespaceIndex);
                abc.Write7BitEncodedInt(NameIndex);

                return abc.ToArray();
            }
        }
    }
}