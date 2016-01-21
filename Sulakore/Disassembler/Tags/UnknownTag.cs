using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
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