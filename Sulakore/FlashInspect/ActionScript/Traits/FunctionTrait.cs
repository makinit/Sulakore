using FlashInspect.IO;

namespace FlashInspect.ActionScript.Traits
{
    public class FunctionTrait : ITrait
    {
        private readonly ABCFile _abc;

        public int SlotId { get; set; }

        public string ObjName { get; }
        public TraitType TraitType => TraitType.Function;
        
        public ASMethod Function
        {
            get { return _abc.Methods[FunctionIndex]; }
        }
        public int FunctionIndex { get; set; }

        public FunctionTrait(ABCFile abc, string objName)
        {
            _abc = abc;

            ObjName = objName;
        }
        public FunctionTrait(ABCFile abc, FlashReader reader, string objName) :
            this(abc, objName)
        {
            SlotId = reader.Read7BitEncodedInt();
            FunctionIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(SlotId);
                abc.Write7BitEncodedInt(FunctionIndex);

                return abc.ToArray();
            }
        }
    }
}