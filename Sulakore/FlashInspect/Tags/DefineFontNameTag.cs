using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    public class DefineFontNameTag : FlashTag
    {
        public ushort FontId { get; set; }
        public string FontName { get; set; }
        public string FontCopyright { get; set; }

        public DefineFontNameTag() :
            this(0, string.Empty, string.Empty)
        { }
        public DefineFontNameTag(ushort fontId, string fontName, string fontCopyright) :
            base(FlashTagType.DefineFontName)
        {
            FontId = fontId;
            FontName = fontName;
            FontCopyright = fontCopyright;
        }

        public DefineFontNameTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            FontId = reader.ReadUInt16();
            FontName = reader.ReadNullTerminatedString();
            FontCopyright = reader.ReadNullTerminatedString();
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter())
            {
                tag.Write(FontId);
                tag.WriteNullTerminatedString(FontName);
                tag.WriteNullTerminatedString(FontCopyright);

                return tag.ToArray();
            }
        }
    }
}