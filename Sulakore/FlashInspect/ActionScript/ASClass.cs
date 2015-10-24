using System.Diagnostics;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.ActionScript.Traits;

namespace FlashInspect.ActionScript
{
    [DebuggerDisplay("{Instance.Name.ObjName} | Traits: {Traits.Count}")]
    public class ASClass : TraitContainer
    {
        private readonly ABCFile _abc;

        public override List<ASTrait> Traits { get; }

        public ASMethod Constructor
        {
            get { return _abc.Methods[ConstructorIndex]; }
        }
        public int ConstructorIndex { get; set; }

        public ASInstance Instance { get; internal set; }

        public ASClass(ABCFile abc)
        {
            _abc = abc;
            Traits = new List<ASTrait>();
        }
        public ASClass(ABCFile abc, int constructorIndex) :
            this(abc)
        {
            ConstructorIndex = constructorIndex;
        }
        public ASClass(ABCFile abc, int constructorIndex, IList<ASTrait> traits) :
            this(abc, constructorIndex)
        {
            Traits.AddRange(traits);
        }

        public ASClass(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            ConstructorIndex = reader.Read7BitEncodedInt();
            Traits = new List<ASTrait>(reader.Read7BitEncodedInt());

            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(ConstructorIndex);
                abc.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    abc.Write(trait.ToArray());

                return abc.ToArray();
            }
        }
    }
}