using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript.Multinames
{
    public interface IMultiname : IABCChild
    {
        ConstantType MultinameType { get; }

        byte[] ToByteArray();
    }
}