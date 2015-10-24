using System.Collections.Generic;

using FlashInspect.IO;

namespace FlashInspect.ActionScript
{
    public class ASMethodBody
    {
        private readonly ABCFile _abc;

        public List<ASTrait> Traits { get; }
        public List<ASException> Exceptions { get; }

        public ASCode Code { get; set; }
        public int MaxStack { get; set; }
        public int LocalCount { get; set; }

        public ASMethod Method
        {
            get { return _abc.Methods[MethodIndex]; }
        }
        public int MethodIndex { get; set; }

        public int MaxScopeDepth { get; set; }
        public int InitialScopeDepth { get; set; }

        public ASMethodBody(ABCFile abc)
        {
            _abc = abc;
            Code = new ASCode(abc, this);
        }
        public ASMethodBody(ABCFile abc, int methodIndex) :
            this(abc)
        {
            MethodIndex = methodIndex;
        }

        public ASMethodBody(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            MethodIndex = reader.Read7BitEncodedInt();

            MaxStack = reader.Read7BitEncodedInt();
            LocalCount = reader.Read7BitEncodedInt();
            InitialScopeDepth = reader.Read7BitEncodedInt();
            MaxScopeDepth = reader.Read7BitEncodedInt();

            Code = new ASCode(abc, this,
                reader.ReadBytes(reader.Read7BitEncodedInt()));

            Exceptions = new List<ASException>(reader.Read7BitEncodedInt());
            for (int i = 0; i < Exceptions.Capacity; i++)
                Exceptions.Add(new ASException(abc, reader));

            Traits = new List<ASTrait>(reader.Read7BitEncodedInt());
            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));

            Method.Body = this;
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(MethodIndex);
                abc.Write7BitEncodedInt(MaxStack);
                abc.Write7BitEncodedInt(LocalCount);
                abc.Write7BitEncodedInt(InitialScopeDepth);
                abc.Write7BitEncodedInt(MaxScopeDepth);

                abc.Write7BitEncodedInt(Code.Count);
                abc.Write(Code.ToArray());

                abc.Write7BitEncodedInt(Exceptions.Count);
                foreach (ASException exception in Exceptions)
                    abc.Write(exception.ToArray());

                abc.Write7BitEncodedInt(Traits.Count);
                foreach (ASTrait trait in Traits)
                    abc.Write(trait.ToArray());

                return abc.ToArray();
            }
        }
    }
}