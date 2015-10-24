using System.Diagnostics;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript
{
    [DebuggerDisplay("{ObjName} | Parameters: {Parameters.Count}")]
    public class ASMethod
    {
        private readonly ABCFile _abc;

        public List<ASParameter> Parameters { get; }

        public string Name
        {
            get { return _abc.Constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ASMultiname ReturnType
        {
            get { return _abc.Constants.Multinames[ReturnTypeIndex]; }
        }
        public int ReturnTypeIndex { get; set; }

        public MethodFlags MethodInfo { get; set; }

        public string ObjName { get; internal set; }
        public ASMethodBody Body { get; internal set; }

        public ASMethod(ABCFile abc)
        {
            _abc = abc;
            Parameters = new List<ASParameter>();
        }
        public ASMethod(ABCFile abc, int returnTypeIndex) :
            this(abc)
        {
            ReturnTypeIndex = returnTypeIndex;
        }
        public ASMethod(ABCFile abc, int returnTypeIndex, IList<ASParameter> parameters) :
            this(abc, returnTypeIndex)
        {
            Parameters.AddRange(parameters);
            if (Parameters.Count > 0)
            {
                ASParameter lastParameter =
                    Parameters[(Parameters.Count) - 1];

                if (lastParameter.NameIndex != 0)
                    MethodInfo |= MethodFlags.HasParamNames;

                if (lastParameter.IsOptional)
                    MethodInfo |= MethodFlags.HasOptional;
            }
        }

        public ASMethod(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            Parameters = new List<ASParameter>(reader.Read7BitEncodedInt());
            ReturnTypeIndex = reader.Read7BitEncodedInt();

            for (int i = 0; i < Parameters.Capacity; i++)
            {
                int parameterTypeIndex = reader.Read7BitEncodedInt();
                Parameters.Add(new ASParameter(abc, parameterTypeIndex));
            }

            NameIndex = reader.Read7BitEncodedInt();
            MethodInfo = (MethodFlags)reader.ReadByte();

            if ((MethodInfo & MethodFlags.HasOptional) != 0)
            {
                int optionalParamCount = reader.Read7BitEncodedInt();
                while (optionalParamCount > 0)
                {
                    int paramIndex = ((Parameters.Count - 1) - (--optionalParamCount));
                    ASParameter optionalParameter = Parameters[paramIndex];

                    optionalParameter.IsOptional = true;
                    optionalParameter.ValueIndex = reader.Read7BitEncodedInt();
                    optionalParameter.ValueType = (ConstantType)reader.ReadByte();
                }
            }

            if ((MethodInfo & MethodFlags.HasParamNames) != 0)
            {
                foreach (ASParameter parameter in Parameters)
                    parameter.NameIndex = reader.Read7BitEncodedInt();
            }
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(Parameters.Count);
                abc.Write7BitEncodedInt(ReturnTypeIndex);

                foreach (ASParameter parameter in Parameters)
                    abc.Write7BitEncodedInt(parameter.TypeIndex);

                abc.Write7BitEncodedInt(NameIndex);
                abc.Write7BitEncodedInt((byte)MethodInfo);

                if ((MethodInfo & MethodFlags.HasOptional) != 0)
                {
                    int optionalParamCount = 0;
                    byte[] optionalParamData = null;
                    using (var opParam = new FlashWriter())
                    {
                        for (int i = 0; i < Parameters.Count; i++)
                        {
                            if (!Parameters[i].IsOptional) continue;

                            optionalParamCount++;
                            opParam.Write7BitEncodedInt(Parameters[i].ValueIndex);
                            opParam.Write((byte)Parameters[i].ValueType);
                        }
                        optionalParamData = opParam.ToArray();
                    }

                    abc.Write7BitEncodedInt(optionalParamCount);
                    abc.Write(optionalParamData);
                }

                if ((MethodInfo & MethodFlags.HasParamNames) != 0)
                {
                    foreach (ASParameter parameter in Parameters)
                        abc.Write7BitEncodedInt(parameter.NameIndex);
                }

                return abc.ToArray();
            }
        }
    }
}