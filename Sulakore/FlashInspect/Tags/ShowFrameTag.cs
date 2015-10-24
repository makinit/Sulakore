using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    public class ShowFrameTag : FlashTag
    {
        public ShowFrameTag() :
            base(FlashTagType.ShowFrame)
        { }

        public ShowFrameTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        { }
    }
}