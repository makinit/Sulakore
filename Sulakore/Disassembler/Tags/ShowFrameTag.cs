using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
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