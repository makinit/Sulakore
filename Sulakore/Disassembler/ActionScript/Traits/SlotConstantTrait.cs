using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Type: {Type?.ObjName}, Value: {Value}, TraitType: {TraitType}, Slot: {Id}")]
    public class SlotConstantTrait : IValueSlot, ITrait
    {
        public int Id => SlotId;
        public ABCFile ABC { get; }
        public TraitType TraitType { get; }

        public ASMultiname Type
        {
            get { return ABC.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public object Value
        {
            get { return ABC.Constants.GetValue(ValueType, ValueIndex); }
        }
        public int ValueIndex { get; set; }

        public int SlotId { get; set; }
        public ConstantType ValueType { get; set; }
        public string ObjName { get; internal set; }

        public SlotConstantTrait(ABCFile abc, TraitType traitType)
        {
            ABC = abc;
            TraitType = traitType;
        }
        public SlotConstantTrait(ABCFile abc, FlashReader reader, TraitType traitType)
            : this(abc, traitType)
        {
            SlotId = reader.Read7BitEncodedInt();
            TypeIndex = reader.Read7BitEncodedInt();
            ValueIndex = reader.Read7BitEncodedInt();

            if (ValueIndex != 0)
                ValueType = (ConstantType)reader.ReadByte();
        }

        public byte[] ToByteArray()
        {
            using (var trait = new FlashWriter())
            {
                trait.Write7BitEncodedInt(SlotId);
                trait.Write7BitEncodedInt(TypeIndex);
                trait.Write7BitEncodedInt(ValueIndex);

                if (ValueIndex != 0)
                    trait.Write((byte)ValueType);

                return trait.ToArray();
            }
        }
    }
}