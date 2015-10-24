namespace FlashInspect.ActionScript.Multinames
{
    public class RTQNameL : IMultiname
    {
        public ConstantType MultinameType { get; }

        public RTQNameL() :
            this(ConstantType.RTQNameL)
        { }
        public RTQNameL(ConstantType multinameType)
        {
            MultinameType = multinameType;
        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}