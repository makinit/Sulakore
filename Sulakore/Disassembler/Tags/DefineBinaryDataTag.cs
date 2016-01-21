using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;
using Sulakore.Disassembler.Dictionary;

namespace Sulakore.Disassembler.Tags
{
    /// <summary>
    /// Represents arbitrary binary data embedded in a flash file.
    /// </summary>
    public class DefineBinaryDataTag : FlashTag, ICharacter
    {
        /// <summary>
        /// Gets or sets the blob of binary data.
        /// </summary>
        public byte[] BinaryData { get; set; }
        /// <summary>
        /// Gets the character id that represents the key in the global dictionary.
        /// </summary>
        public ushort CharacterId { get; set; }

        public DefineBinaryDataTag() :
            this(0, new byte[0])
        { }
        public DefineBinaryDataTag(byte[] binaryData) :
            this(0, binaryData)
        { }
        public DefineBinaryDataTag(ushort characterId, byte[] binaryData) :
            base(FlashTagType.DefineBinaryData)
        {
            BinaryData = binaryData;
            CharacterId = characterId;
        }

        public DefineBinaryDataTag(FlashReader reader, TagRecord header)
            : base(reader, header)
        {
            CharacterId = reader.ReadUInt16();
            reader.ReadUInt32();

            BinaryData = reader.ReadBytes(header.Body.Length - 6);
        }

        protected override byte[] OnConstruct()
        {
            using (var tag =
                new FlashWriter(6 + BinaryData.Length))
            {
                tag.Write(CharacterId);
                tag.Position += 4;
                tag.Write(BinaryData);

                return tag.ToArray();
            }
        }
    }
}