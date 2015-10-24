using System;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    public class ExportAssetsTag : FlashTag
    {
        public Dictionary<ushort, string> Assets { get; }

        public ExportAssetsTag() :
            base(FlashTagType.ExportAssets)
        {
            Assets = new Dictionary<ushort, string>();
        }
        public ExportAssetsTag(IDictionary<ushort, string> assets) :
            this()
        {
            foreach (KeyValuePair<ushort, string> asset in assets)
                Assets.Add(asset.Key, asset.Value);
        }

        public ExportAssetsTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            int count = reader.ReadUInt16();
            Assets = new Dictionary<ushort, string>(count);

            for (int i = 0; i < count; i++)
            {
                ushort tag = reader.ReadUInt16();
                string name = reader.ReadNullTerminatedString();

                if (Assets.ContainsKey(tag))
                {
                    throw new Exception(
                        "Duplicate tag id: " + tag);
                }
                Assets[tag] = name;
            }
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter())
            {
                tag.Write((ushort)Assets.Count);
                foreach (KeyValuePair<ushort, string> pair in Assets)
                {
                    tag.Write(pair.Key);
                    tag.WriteNullTerminatedString(pair.Value);
                }
                return tag.ToArray();
            }
        }
    }
}