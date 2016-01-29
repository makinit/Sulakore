using System.Diagnostics;

using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{ObjName}, Type: {Type?.ObjName}, Value: {Value}, IsOptional: {IsOptional}")]
    public class ASParameter : IValueSlot, IABCChild
    {
        public ABCFile ABC { get; }

        public ASMultiname Type
        {
            get { return ABC.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public object Value
        {
            get { return ABC.Constants.GetValue(ValueType, ValueIndex); }
        }
        public int ValueIndex { get; set; }

        public bool IsOptional { get; set; }
        public ConstantType ValueType { get; set; }

        public int Rank { get; internal set; }

        public ASParameter(ABCFile abc)
            : this(abc, 0, false)
        { }
        public ASParameter(ABCFile abc, int typeIndex)
            : this(abc, typeIndex, false)
        { }
        public ASParameter(ABCFile abc, int typeIndex, bool isOptional)
        {
            ABC = abc;
            TypeIndex = typeIndex;
            IsOptional = isOptional;
        }

        public override string ToString()
        {
            string paramName = ObjName;
            if (string.IsNullOrWhiteSpace(paramName))
                paramName = "arg" + Rank;

            string paramType = Type.ObjName;
            bool isString = (paramType.ToLower() == "string");

            string argument = $"{paramName}:{paramType}";
            if (IsOptional)
            {
                argument += "=";
                if (isString) argument += "\"";

                object paramValue = (Value ?? "null");
                argument += paramValue;

                if (isString) argument += "\"";
            }
            return argument;
        }
    }
}