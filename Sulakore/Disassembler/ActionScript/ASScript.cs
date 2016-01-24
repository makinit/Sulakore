using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Traits;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("Function: {Function?.ObjName}, Traits: {Traits.Count}")]
    public class ASScript : TraitContainer, IABCChild
    {
        public ABCFile ABC { get; }
        public override List<ASTrait> Traits { get; }

        public ASMethod Function
        {
            get { return ABC.Methods[FunctionIndex]; }
        }
        public int FunctionIndex { get; set; }

        public ASScript(ABCFile abc)
        {
            ABC = abc;
            Traits = new List<ASTrait>();
        }
        public ASScript(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            FunctionIndex = reader.Read7BitEncodedInt();
            int traitCount = reader.Read7BitEncodedInt();

            for (int i = 0; i < traitCount; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToByteArray()
        {
            using (var asScript = new FlashWriter())
            {
                asScript.Write7BitEncodedInt(FunctionIndex);
                asScript.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    asScript.Write(trait.ToByteArray());

                return asScript.ToArray();
            }
        }
    }
}