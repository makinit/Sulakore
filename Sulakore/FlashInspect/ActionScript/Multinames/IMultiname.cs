namespace FlashInspect.ActionScript.Multinames
{
    public interface IMultiname
    {
        ConstantType MultinameType { get; }

        byte[] ToArray();
    }
}