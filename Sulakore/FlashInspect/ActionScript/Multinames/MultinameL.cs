using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript.Multinames
{
    public class MultinameL : IMultiname
    {
        private readonly ASConstants _constants;

        public ConstantType MultinameType { get; }

        public ASNamespaceSet NamespaceSet
        {
            get { return _constants.NamespaceSets[NamespaceSetIndex]; }
        }
        public int NamespaceSetIndex { get; set; }

        public MultinameL(ASConstants constants) :
            this(constants, 0, ConstantType.MultinameL)
        { }
        public MultinameL(ASConstants constants, ConstantType multinameType) :
            this(constants, 0, multinameType)
        { }
        public MultinameL(ASConstants constants, int namespaceSetIndex, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NamespaceSetIndex = namespaceSetIndex;
        }

        public MultinameL(ASConstants constants, FlashReader reader) :
            this(constants, reader, ConstantType.MultinameL)
        { }
        public MultinameL(ASConstants constants, FlashReader reader, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NamespaceSetIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NamespaceSetIndex);
                return abc.ToArray();
            }
        }
    }
}