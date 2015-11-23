using System.Diagnostics;

using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}:{Type.ObjName}={Value}")]
    public class SlotConstantTrait : ITrait
    {
        private readonly ABCFile _abc;

        public string ObjName { get; }
        public TraitType TraitType { get; }

        public int SlotId { get; set; }
        public ConstantType ValueType { get; set; }

        public ASMultiname Type
        {
            get { return _abc.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public object Value
        {
            get { return _abc.Constants.GetValue(ValueType, ValueIndex); }
        }
        public int ValueIndex { get; set; }

        public SlotConstantTrait(ABCFile abc, string objName, TraitType traitType)
        {
            _abc = abc;

            ObjName = objName;
            TraitType = traitType;
        }
        public SlotConstantTrait(ABCFile abc, FlashReader reader, string objName, TraitType traitType)
            : this(abc, objName, traitType)
        {
            SlotId = reader.Read7BitEncodedInt();
            TypeIndex = reader.Read7BitEncodedInt();
            ValueIndex = reader.Read7BitEncodedInt();

            if (ValueIndex != 0)
                ValueType = (ConstantType)reader.ReadByte();
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(SlotId);
                abc.Write7BitEncodedInt(TypeIndex);
                abc.Write7BitEncodedInt(ValueIndex);

                if (ValueIndex != 0)
                    abc.Write((byte)ValueType);

                return abc.ToArray();
            }
        }
    }
}