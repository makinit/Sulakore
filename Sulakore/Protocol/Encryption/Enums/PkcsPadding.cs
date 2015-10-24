namespace Sulakore.Protocol.Encryption
{
    public enum PkcsPadding
    {
        /// <summary>
        /// Represents a padding type that will attempt to fill a byte array with the maximum value of a <see cref="byte"/>.
        /// </summary>
        MaxByte = 0,
        /// <summary>
        /// Represents a padding type that will attempt to fill a byte array with random bytes in the range of 1 - 255.
        /// </summary>
        RandomByte = 1
    }
}