using System;

using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    public enum FlashEdition
    {
        Developer = 0,
        FullCommercial = 1,
        NonCommercial = 2,
        Educational = 3,
        NotForResale = 4,
        Trial = 5,
        None = 6
    }
    public enum FlashProductId
    {
        Unknown = 0,
        MacromediaFlexJ2EE = 1,
        MacromediaFlexNET = 2,
        AdobeFlex = 3
    }

    public class ProductInfoTag : FlashTag
    {
        private static readonly DateTime _epoch;

        public FlashEdition Edition { get; set; }
        public FlashProductId ProductId { get; set; }

        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }

        public uint BuildLow { get; set; }
        public uint BuildHigh { get; set; }

        public DateTime CompilationDate { get; set; }

        static ProductInfoTag()
        {
            _epoch = new DateTime(1970, 1, 1);
        }
        public ProductInfoTag() :
            base(FlashTagType.ProductInfo)
        { }

        public ProductInfoTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            ProductId = (FlashProductId)reader.ReadUInt32();
            Edition = (FlashEdition)reader.ReadUInt32();

            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();

            BuildLow = reader.ReadUInt32();
            BuildHigh = reader.ReadUInt32();

            CompilationDate =
                _epoch.AddMilliseconds(reader.ReadUInt64());
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter(26))
            {
                tag.Write((uint)ProductId);
                tag.Write((uint)Edition);

                tag.Write(MajorVersion);
                tag.Write(MinorVersion);

                tag.Write(BuildLow);
                tag.Write(BuildHigh);

                tag.Write((ulong)(
                    (CompilationDate - _epoch).TotalMilliseconds));

                return tag.ToArray();
            }
        }
    }
}