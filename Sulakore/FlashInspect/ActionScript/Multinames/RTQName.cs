using FlashInspect.IO;

namespace FlashInspect.ActionScript.Multinames
{
    public class RTQName : IMultiname
    {
        private readonly ASConstants _constants;

        public ConstantType MultinameType { get; }

        public string Name
        {
            get { return _constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public RTQName(ASConstants constants) :
            this(constants, 0, ConstantType.RTQName)
        { }
        public RTQName(ASConstants constants, ConstantType multinameType) :
            this(constants, 0, multinameType)
        { }
        public RTQName(ASConstants constants, int nameIndex, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NameIndex = nameIndex;
        }

        public RTQName(ASConstants constants, FlashReader reader) :
            this(constants, reader, ConstantType.RTQName)
        { }
        public RTQName(ASConstants constants, FlashReader reader, ConstantType multinameType)
        {
            _constants = constants;
            MultinameType = multinameType;
            NameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NameIndex);
                return abc.ToArray();
            }
        }
    }
}