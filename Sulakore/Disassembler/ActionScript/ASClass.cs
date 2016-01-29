using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Traits;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{Instance.Type.ObjName}, Traits: {Traits.Count}")]
    public class ASClass : TraitContainer, IABCChild
    {
        public ABCFile ABC { get; }
        public override List<ASTrait> Traits { get; }

        public ASMethod Constructor
        {
            get { return ABC.Methods[ConstructorIndex]; }
        }
        public int ConstructorIndex { get; set; }

        public ASInstance Instance { get; internal set; }

        public ASClass(ABCFile abc)
        {
            ABC = abc;
            Traits = new List<ASTrait>();
        }
        public ASClass(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            ConstructorIndex = reader.Read7BitEncodedInt();
            if (Constructor != null) Constructor.IsConstructor = true;

            Traits.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToByteArray()
        {
            using (var asClass = new FlashWriter())
            {
                asClass.Write7BitEncodedInt(ConstructorIndex);
                asClass.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    asClass.Write(trait.ToByteArray());

                return asClass.ToArray();
            }
        }
    }
}