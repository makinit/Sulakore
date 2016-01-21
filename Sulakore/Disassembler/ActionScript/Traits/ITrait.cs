namespace Sulakore.Disassembler.ActionScript.Traits
{
    public interface ITrait : IABCChild
    {
        int Id { get; }
        string ObjName { get; }
        TraitType TraitType { get; }

        byte[] ToByteArray();
    }
}