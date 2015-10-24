using FlashInspect.IO;

namespace FlashInspect.ActionScript.Constants
{
    public class ASNamespace
    {
        private readonly ASConstants _constants;

        public string Name
        {
            get { return _constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ConstantType NamespaceType { get; set; }

        public ASNamespace(ASConstants constants) :
            this(constants, 0)
        { }
        public ASNamespace(ASConstants constants, int nameIndex) :
            this(constants, nameIndex, ConstantType.Namespace)
        { }
        public ASNamespace(ASConstants constants, int nameIndex, ConstantType kind)
        {
            _constants = constants;
            NameIndex = nameIndex;

            NamespaceType = kind;
        }

        public ASNamespace(ASConstants constants, FlashReader reader)
        {
            _constants = constants;

            NamespaceType = (ConstantType)reader.ReadByte();
            NameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write((byte)NamespaceType);
                abc.Write7BitEncodedInt(NameIndex);

                return abc.ToArray();
            }
        }
    }
}