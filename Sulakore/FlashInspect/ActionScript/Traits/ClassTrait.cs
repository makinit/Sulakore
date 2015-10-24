using FlashInspect.IO;

namespace FlashInspect.ActionScript.Traits
{
    public class ClassTrait : ITrait
    {
        private readonly ABCFile _abc;

        public int SlotId { get; set; }

        public string ObjName { get; }
        public TraitType TraitType => TraitType.Class;
        
        public ASClass Class
        {
            get { return _abc.Classes[ClassIndex]; }
        }
        public int ClassIndex { get; set; }

        public ClassTrait(ABCFile abc, string objName)
        {
            _abc = abc;

            ObjName = objName;
        }
        public ClassTrait(ABCFile abc, FlashReader reader, string objName) :
            this(abc, objName)
        {
            SlotId = reader.Read7BitEncodedInt();
            ClassIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(SlotId);
                abc.Write7BitEncodedInt(ClassIndex);

                return abc.ToArray();
            }
        }
    }
}