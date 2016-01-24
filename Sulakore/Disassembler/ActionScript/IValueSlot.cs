using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    public interface IValueSlot
    {
        object Value { get; }
        int ValueIndex { get; set; }
        ConstantType ValueType { get; set; }
    }
}