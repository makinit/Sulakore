using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Function: {Function?.ObjName}, SlotId: {Id}")]
    public class FunctionTrait : ITrait
    {
        public int Id => SlotId;
        public ABCFile ABC { get; }
        public string ObjName { get; }
        public TraitType TraitType => TraitType.Function;

        public ASMethod Function
        {
            get { return ABC.Methods[FunctionIndex]; }
        }
        public int FunctionIndex { get; set; }

        public int SlotId { get; set; }

        public FunctionTrait(ABCFile abc, string objName)
        {
            ABC = abc;
            ObjName = objName;
        }
        public FunctionTrait(ABCFile abc, FlashReader reader, string objName)
            : this(abc, objName)
        {
            SlotId = reader.Read7BitEncodedInt();
            FunctionIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var trait = new FlashWriter())
            {
                trait.Write7BitEncodedInt(SlotId);
                trait.Write7BitEncodedInt(FunctionIndex);
                return trait.ToArray();
            }
        }
    }
}