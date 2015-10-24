namespace FlashInspect.ActionScript.Traits
{
    public interface ITrait
    {
        string ObjName { get; }
        TraitType TraitType { get; }

        byte[] ToArray();
    }
}