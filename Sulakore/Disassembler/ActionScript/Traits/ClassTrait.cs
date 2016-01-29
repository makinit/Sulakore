using System.Diagnostics;
using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Class: {Class?.ObjName}, SlotId: {Id}")]
    public class ClassTrait : ITrait
    {
        public int Id => SlotId;
        public ABCFile ABC { get; }
        public TraitType TraitType => TraitType.Class;

        public ASClass Class
        {
            get { return ABC.Classes[ClassIndex]; }
        }
        public int ClassIndex { get; set; }

        public int SlotId { get; set; }
        public string ObjName { get; internal set; }

        public ClassTrait(ABCFile abc)
        {
            ABC = abc;
        }
        public ClassTrait(ABCFile abc, FlashReader reader) :
            this(abc)
        {
            SlotId = reader.Read7BitEncodedInt();
            ClassIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var trait = new FlashWriter())
            {
                trait.Write7BitEncodedInt(SlotId);
                trait.Write7BitEncodedInt(ClassIndex);
                return trait.ToArray();
            }
        }
    }
}