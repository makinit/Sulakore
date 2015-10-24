using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    public class UnknownTag : FlashTag
    {
        public UnknownTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            reader.Position += header.Body.Length;
        }
    }
}