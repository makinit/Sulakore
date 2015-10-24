namespace FlashInspect.Dictionary
{
    public interface ICharacter
    {
        /// <summary>
        /// Gets the character id that represents the key in the global dictionary.
        /// </summary>
        ushort CharacterId { get; }
    }
}