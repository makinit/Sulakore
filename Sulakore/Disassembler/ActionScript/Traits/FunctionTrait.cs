using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Function: {Function?.ObjName}, SlotId: {Id}")]
    public class FunctionTrait : ITrait
    {
        public int Id => SlotId;
        public ABCFile ABC { get; }
        public TraitType TraitType => TraitType.Function;

        public ASMethod Function
        {
            get { return ABC.Methods[FunctionIndex]; }
        }
        public int FunctionIndex { get; set; }

        public int SlotId { get; set; }
        public string ObjName { get; internal set; }

        public FunctionTrait(ABCFile abc)
        {
            ABC = abc;
        }
        public FunctionTrait(ABCFile abc, FlashReader reader)
            : this(abc)
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