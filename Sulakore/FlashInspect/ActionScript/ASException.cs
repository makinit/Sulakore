using FlashInspect.IO;

namespace FlashInspect.ActionScript
{
    public class ASException
    {
        private readonly ABCFile _abc;

        public int To { get; set; }
        public int From { get; set; }
        public int Target { get; set; }

        public string VariableName
        {
            get { return _abc.Constants.Strings[VariableNameIndex]; }
        }
        public int VariableNameIndex { get; set; }

        public string ExceptionType
        {
            get { return _abc.Constants.Strings[ExceptionTypeIndex]; }
        }
        public int ExceptionTypeIndex { get; set; }

        public ASException(ABCFile abc)
        {
            _abc = abc;
        }
        public ASException(ABCFile abc, int variableNameIndex, int exceptionTypeIndex) :
            this(abc)
        {
            VariableNameIndex = variableNameIndex;
            ExceptionTypeIndex = exceptionTypeIndex;
        }

        public ASException(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            From = reader.Read7BitEncodedInt();
            To = reader.Read7BitEncodedInt();
            Target = reader.Read7BitEncodedInt();
            ExceptionTypeIndex = reader.Read7BitEncodedInt();
            VariableNameIndex = reader.Read7BitEncodedInt();
        }
        

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(From);
                abc.Write7BitEncodedInt(To);
                abc.Write7BitEncodedInt(Target);
                abc.Write7BitEncodedInt(ExceptionTypeIndex);
                abc.Write7BitEncodedInt(VariableNameIndex);

                return abc.ToArray();
            }
        }
    }
}