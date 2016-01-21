using System;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
{
    public abstract class FlashTag
    {
        public TagRecord Header { get; }

        public FlashTag(FlashTagType tagType)
        {
            Header = new TagRecord(tagType);
        }
        public FlashTag(FlashReader reader, TagRecord header)
        {
            Header = header;
        }

        public byte[] ToArray()
        {
            Header.Body = OnConstruct();

            var header = ((uint)Header.TagType << 6);
            header |= (Header.IsLong ? 63 : (uint)Header.Body.Length);

            byte[] tagData = new byte[2 + Header.Body.Length + (Header.IsLong ? 4 : 0)];
            byte[] headerData = BitConverter.GetBytes((ushort)header);
            Buffer.BlockCopy(headerData, 0, tagData, 0, 2);

            if (Header.IsLong)
            {
                byte[] lengthData = BitConverter.GetBytes(Header.Body.Length);
                Buffer.BlockCopy(lengthData, 0, tagData, 2, 4);
            }
            Buffer.BlockCopy(Header.Body, 0, tagData,
                Header.IsLong ? 6 : 2, Header.Body.Length);

            return tagData;
        }
        protected virtual byte[] OnConstruct() => Header.Body;
    }
}