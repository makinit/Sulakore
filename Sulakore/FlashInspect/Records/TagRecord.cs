using FlashInspect.IO;
using FlashInspect.Tags;

namespace FlashInspect.Records
{
    public class TagRecord
    {
        public byte[] Body { get; set; }

        public bool IsLong
        {
            get
            {
                if (TagType == FlashTagType.DoInitAction ||
                    TagType == FlashTagType.DefineBitsLossless2 ||
                    TagType == FlashTagType.DefineBitsJPEG3)
                    return true;

                return IsSpecialLong || Body.Length >= 63;
            }
        }
        public bool IsSpecialLong { get; }

        public int Start { get; }
        public int BodyStart { get; }
        public FlashTagType TagType { get; }

        public TagRecord(FlashReader reader)
        {
            Start = reader.Position;

            ushort header = reader.ReadUInt16();
            TagType = (FlashTagType)(header >> 6);

            int length = (header & 63);
            if (length >= 63)
            {
                length = reader.ReadInt32();
                IsSpecialLong = (length < 63);
            }
            BodyStart = reader.Position;

            Body = reader.ReadBytes(length);
            reader.Position = BodyStart;
        }
        public TagRecord(FlashTagType tagType)
        {
            TagType = tagType;
            Body = new byte[0];
        }
    }
}