using FlashInspect.IO;

namespace FlashInspect.ActionScript.Traits
{
    public class MethodGetterSetterTrait : ITrait
    {
        private readonly ABCFile _abc;

        public int DispId { get; set; }

        public string ObjName { get; }
        public TraitType TraitType { get; }

        public ASMethod Method
        {
            get { return _abc.Methods[MethodIndex]; }
        }
        public int MethodIndex { get; set; }

        public MethodGetterSetterTrait(ABCFile abc, string objName, TraitType traitType)
        {
            _abc = abc;

            ObjName = objName;
            TraitType = traitType;
        }
        public MethodGetterSetterTrait(ABCFile abc, FlashReader reader, string objName, TraitType traitType) :
            this(abc, objName, traitType)
        {
            DispId = reader.Read7BitEncodedInt();
            MethodIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(DispId);
                abc.Write7BitEncodedInt(MethodIndex);

                return abc.ToArray();
            }
        }
    }
}