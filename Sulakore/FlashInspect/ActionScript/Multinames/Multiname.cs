using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript.Multinames
{
    public class Multiname : IMultiname
    {
        private readonly ASConstants _constants;

        public ConstantType MultinameType { get; }

        public string Name
        {
            get { return _constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ASNamespaceSet NamespaceSet
        {
            get { return _constants.NamespaceSets[NamespaceSetIndex]; }
        }
        public int NamespaceSetIndex { get; set; }

        public Multiname(ASConstants constants) :
            this(constants, 0, 0, ConstantType.Multiname)
        { }
        public Multiname(ASConstants constants, ConstantType multinameType) :
            this(constants, 0, 0, multinameType)
        { }
        public Multiname(ASConstants constants, int nameIndex, int namespaceSetIndex, ConstantType multinameType)
        {
            _constants = constants;

            NameIndex = nameIndex;
            MultinameType = multinameType;
            NamespaceSetIndex = namespaceSetIndex;
        }

        public Multiname(ASConstants constants, FlashReader reader) :
            this(constants, reader, ConstantType.Multiname)
        { }
        public Multiname(ASConstants constants, FlashReader reader, ConstantType multinameType)
        {
            _constants = constants;

            MultinameType = multinameType;
            NameIndex = reader.Read7BitEncodedInt();
            NamespaceSetIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NameIndex);
                abc.Write7BitEncodedInt(NamespaceSetIndex);

                return abc.ToArray();
            }
        }
    }
}