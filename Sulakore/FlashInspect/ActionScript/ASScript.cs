using System.Collections.Generic;

using FlashInspect.IO;

namespace FlashInspect.ActionScript
{
    public class ASScript
    {
        private readonly ABCFile _abc;

        public List<ASTrait> Traits { get; }

        public ASMethod Function
        {
            get { return _abc.Methods[FunctionIndex]; }
        }
        public int FunctionIndex { get; set; }

        public ASScript(ABCFile abc)
        {
            _abc = abc;
            Traits = new List<ASTrait>();
        }
        public ASScript(ABCFile abc, int functionIndex) :
            this(abc)
        {
            FunctionIndex = functionIndex;
        }
        public ASScript(ABCFile abc, int functionIndex, IList<ASTrait> traits) :
            this(abc, functionIndex)
        {
            Traits.AddRange(traits);
        }

        public ASScript(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            FunctionIndex =
                reader.Read7BitEncodedInt();

            Traits = new List<ASTrait>(reader.Read7BitEncodedInt());
            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(FunctionIndex);
                abc.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    abc.Write(trait.ToArray());

                return abc.ToArray();
            }
        }
    }
}