using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Method: {Method?.ObjName}, TraitType: {TraitType}, DispId: {Id}")]
    public class MethodGetterSetterTrait : ITrait
    {
        public int Id => DispId;
        public ABCFile ABC { get; }
        public TraitType TraitType { get; }

        public ASMethod Method
        {
            get { return ABC.Methods[MethodIndex]; }
        }
        public int MethodIndex { get; set; }

        public int DispId { get; set; }
        public string ObjName { get; internal set; }

        public MethodGetterSetterTrait(ABCFile abc, TraitType traitType)
        {
            ABC = abc;
            TraitType = traitType;
        }
        public MethodGetterSetterTrait(ABCFile abc, FlashReader reader, TraitType traitType)
            : this(abc, traitType)
        {
            DispId = reader.Read7BitEncodedInt();
            MethodIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var trait = new FlashWriter())
            {
                trait.Write7BitEncodedInt(DispId);
                trait.Write7BitEncodedInt(MethodIndex);
                return trait.ToArray();
            }
        }
    }
}