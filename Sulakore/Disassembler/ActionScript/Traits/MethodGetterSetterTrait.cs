using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    [DebuggerDisplay("{ObjName}, Method: {Method?.ObjName}, TraitType: {TraitType}, DispId: {Id}")]
    public class MethodGetterSetterTrait : ITrait
    {
        public int Id => DispId;
        public ABCFile ABC { get; }
        public string ObjName { get; }
        public TraitType TraitType { get; }

        public ASMethod Method
        {
            get { return ABC.Methods[MethodIndex]; }
        }
        public int MethodIndex { get; set; }

        public int DispId { get; set; }

        public MethodGetterSetterTrait(ABCFile abc, string objName, TraitType traitType)
        {
            ABC = abc;
            ObjName = objName;
            TraitType = traitType;
        }
        public MethodGetterSetterTrait(ABCFile abc, FlashReader reader, string objName, TraitType traitType) :
            this(abc, objName, traitType)
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