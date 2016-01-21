using System.Diagnostics;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("To: {To}, From: {From}, Target: {Target}, VariableName: {VariableName}, ExceptionType: {ExceptionType}")]
    public class ASException : IABCChild
    {
        public ABCFile ABC { get; }

        public int To { get; set; }
        public int From { get; set; }
        public int Target { get; set; }

        public string VariableName
        {
            get { return ABC.Constants.Strings[VariableNameIndex]; }
        }
        public int VariableNameIndex { get; set; }

        public string ExceptionType
        {
            get { return ABC.Constants.Strings[ExceptionTypeIndex]; }
        }
        public int ExceptionTypeIndex { get; set; }

        public ASException(ABCFile abc)
        {
            ABC = abc;
        }
        public ASException(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            From = reader.Read7BitEncodedInt();
            To = reader.Read7BitEncodedInt();
            Target = reader.Read7BitEncodedInt();
            ExceptionTypeIndex = reader.Read7BitEncodedInt();
            VariableNameIndex = reader.Read7BitEncodedInt();
        }

        public byte[] ToByteArray()
        {
            using (var asException = new FlashWriter())
            {
                asException.Write7BitEncodedInt(From);
                asException.Write7BitEncodedInt(To);
                asException.Write7BitEncodedInt(Target);
                asException.Write7BitEncodedInt(ExceptionTypeIndex);
                asException.Write7BitEncodedInt(VariableNameIndex);
                return asException.ToArray();
            }
        }
    }
}