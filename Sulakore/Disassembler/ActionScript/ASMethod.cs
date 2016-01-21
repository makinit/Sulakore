using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{ObjName}, ReturnType: {ReturnType?.ObjName}, Parameters: {Parameters.Count}, IsConstructor: {IsConstructor}")]
    public class ASMethod : IABCChild
    {
        public ABCFile ABC { get; }
        public List<ASParameter> Parameters { get; }

        public MethodFlags MethodInfo { get; set; }

        public string ObjName { get; internal set; }
        public ASMethodBody Body { get; internal set; }
        public bool IsConstructor { get; internal set; }

        public string Name
        {
            get { return ABC.Constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ASMultiname ReturnType
        {
            get { return ABC.Constants.Multinames[ReturnTypeIndex]; }
        }
        public int ReturnTypeIndex { get; set; }

        public ASMethod(ABCFile abc)
        {
            ABC = abc;
            Parameters = new List<ASParameter>();
        }
        public ASMethod(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            int parameterCount = reader.Read7BitEncodedInt();
            ReturnTypeIndex = reader.Read7BitEncodedInt();

            for (int i = 0; i < parameterCount; i++)
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
                    parameter.ObjNameIndex = reader.Read7BitEncodedInt();
            }
        }

        public byte[] ToByteArray()
        {
            using (var asMethod = new FlashWriter())
            {
                asMethod.Write7BitEncodedInt(Parameters.Count);
                asMethod.Write7BitEncodedInt(ReturnTypeIndex);

                foreach (ASParameter parameter in Parameters)
                    asMethod.Write7BitEncodedInt(parameter.TypeIndex);

                asMethod.Write7BitEncodedInt(NameIndex);
                asMethod.Write7BitEncodedInt((byte)MethodInfo);

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

                    asMethod.Write7BitEncodedInt(optionalParamCount);
                    asMethod.Write(optionalParamData);
                }

                if ((MethodInfo & MethodFlags.HasParamNames) != 0)
                {
                    foreach (ASParameter parameter in Parameters)
                        asMethod.Write7BitEncodedInt(parameter.ObjNameIndex);
                }
                return asMethod.ToArray();
            }
        }
    }
}