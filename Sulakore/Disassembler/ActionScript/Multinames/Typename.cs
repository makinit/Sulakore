using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    [DebuggerDisplay("Type: {Type?.ObjName}, ParameterTypeIndices: {ParameterTypeIndices}, MultinameType: {MultinameType}")]
    public class Typename : IMultiname
    {
        public ABCFile ABC { get; }
        public List<int> ParameterTypeIndices { get; }
        public ConstantType MultinameType => ConstantType.Typename;

        public ASMultiname Type
        {
            get { return ABC.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public Typename(ABCFile abc)
        {
            ABC = abc;
            ParameterTypeIndices = new List<int>();
        }
        public Typename(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            TypeIndex = reader.Read7BitEncodedInt();
            int paramTypeIndicesCount = reader.Read7BitEncodedInt();

            for (int i = 0; i < paramTypeIndicesCount; i++)
                ParameterTypeIndices.Add(reader.Read7BitEncodedInt());
        }

        public byte[] ToByteArray()
        {
            using (var typename = new FlashWriter())
            {
                typename.Write7BitEncodedInt(TypeIndex);
                typename.Write7BitEncodedInt(ParameterTypeIndices.Count);

                foreach (int multinameParameterIndex in ParameterTypeIndices)
                    typename.Write7BitEncodedInt(multinameParameterIndex);

                return typename.ToArray();
            }
        }
    }
}