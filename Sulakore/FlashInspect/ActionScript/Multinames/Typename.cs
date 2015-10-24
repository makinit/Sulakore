using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

using System.Collections.Generic;

namespace FlashInspect.ActionScript.Multinames
{
    public class Typename : IMultiname
    {
        private readonly ASConstants _constants;

        public ConstantType MultinameType =>
            ConstantType.Typename;

        public ASMultiname Type
        {
            get { return _constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public List<int> ParameterTypeIndices { get; }

        public Typename(ASConstants constants)
        {
            _constants = constants;
            ParameterTypeIndices = new List<int>();
        }
        public Typename(ASConstants constants, int typeIndex) :
            this(constants)
        {
            TypeIndex = typeIndex;
        }
        public Typename(ASConstants constants, int typeIndex, IList<int> parameterTypeIndices) :
            this(constants, typeIndex)
        {
            ParameterTypeIndices.AddRange(parameterTypeIndices);
        }

        public Typename(ASConstants constants, FlashReader reader)
        {
            _constants = constants;

            TypeIndex = reader.Read7BitEncodedInt();
            ParameterTypeIndices = new List<int>(reader.Read7BitEncodedInt());

            for (int i = 0; i < ParameterTypeIndices.Capacity; i++)
                ParameterTypeIndices.Add(reader.Read7BitEncodedInt());
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(TypeIndex);
                abc.Write7BitEncodedInt(ParameterTypeIndices.Count);

                foreach (int multinameParameterIndex in ParameterTypeIndices)
                    abc.Write7BitEncodedInt(multinameParameterIndex);

                return abc.ToArray();
            }
        }
    }
}