using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Traits;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("MaxStack: {MaxStack}, LocalCount: {LocalCount}, MaxScopeDepth: {MaxScopeDepth}, InitialScopeDepth: {InitialScopeDepth}")]
    public class ASMethodBody : TraitContainer, IABCChild
    {
        public ABCFile ABC { get; }
        public List<ASException> Exceptions { get; }
        public override List<ASTrait> Traits { get; }

        public int MaxStack { get; set; }
        public int LocalCount { get; set; }
        public byte[] Bytecode { get; set; }

        public ASMethod Method
        {
            get { return ABC.Methods[MethodIndex]; }
        }
        public int MethodIndex { get; set; }

        public int MaxScopeDepth { get; set; }
        public int InitialScopeDepth { get; set; }

        public ASMethodBody(ABCFile abc)
        {
            ABC = abc;
            Traits = new List<ASTrait>();
            Exceptions = new List<ASException>();
        }
        public ASMethodBody(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            MethodIndex = reader.Read7BitEncodedInt();
            MaxStack = reader.Read7BitEncodedInt();
            LocalCount = reader.Read7BitEncodedInt();
            InitialScopeDepth = reader.Read7BitEncodedInt();
            MaxScopeDepth = reader.Read7BitEncodedInt();

            int bytecodeLength = reader.Read7BitEncodedInt();
            Bytecode = reader.ReadBytes(bytecodeLength);

            Exceptions.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < Exceptions.Capacity; i++)
                Exceptions.Add(new ASException(abc, reader));

            Traits.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));

            Method.Body = this;
        }

        public byte[] ToByteArray()
        {
            using (var asMethodBody = new FlashWriter())
            {
                asMethodBody.Write7BitEncodedInt(MethodIndex);
                asMethodBody.Write7BitEncodedInt(MaxStack);
                asMethodBody.Write7BitEncodedInt(LocalCount);
                asMethodBody.Write7BitEncodedInt(InitialScopeDepth);
                asMethodBody.Write7BitEncodedInt(MaxScopeDepth);

                asMethodBody.Write7BitEncodedInt(Bytecode.Length);
                asMethodBody.Write(Bytecode);

                asMethodBody.Write7BitEncodedInt(Exceptions.Count);
                foreach (ASException exception in Exceptions)
                    asMethodBody.Write(exception.ToByteArray());

                asMethodBody.Write7BitEncodedInt(Traits.Count);
                foreach (ASTrait trait in Traits)
                    asMethodBody.Write(trait.ToByteArray());

                return asMethodBody.ToArray();
            }
        }
    }
}