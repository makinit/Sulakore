using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript
{
    public class ASParameter
    {
        private readonly ABCFile _abc;

        public ASMultiname Type
        {
            get { return _abc.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public string Name
        {
            get { return _abc.Constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public object Value
        {
            get { return _abc.Constants.GetValue(ValueType, ValueIndex); }
        }
        public int ValueIndex { get; set; }
        public bool IsOptional { get; set; }
        public ConstantType ValueType { get; set; }

        public ASParameter(ABCFile abc)
        {
            _abc = abc;
        }
        public ASParameter(ABCFile abc, int typeIndex) :
            this(abc)
        {
            TypeIndex = typeIndex;
        }
        public ASParameter(ABCFile abc, int typeIndex, bool isOptional) :
            this(abc, typeIndex)
        {
            IsOptional = isOptional;
        }
    }
}