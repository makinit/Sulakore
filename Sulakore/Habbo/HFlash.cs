using System.IO;
using System.Collections.Generic;

using FlashInspect;
using FlashInspect.IO;
using FlashInspect.Tags;
using FlashInspect.Records;

namespace Sulakore.Habbo
{
    public sealed class HFlash : ShockwaveFlash
    {
        private readonly IList<DoABCTag> _abcTags;
        private readonly IList<DefineBinaryDataTag> _binTags;

        public HFlash(byte[] data) :
            base(data)
        {
            _abcTags = new List<DoABCTag>();
            _binTags = new List<DefineBinaryDataTag>();
        }
        public HFlash(string path) :
            this(File.ReadAllBytes(path))
        { }

        public bool DisableClientEncryption()
        {
            return false;
        }
        public bool RemoveLocalUseRestrictions()
        {
            return false;
        }

        public override List<FlashTag> ReadTags()
        {
            _abcTags.Clear();
            _binTags.Clear();
            return base.ReadTags();
        }

        protected override FlashTag ReadTag(FlashReader reader, TagRecord header)
        {
            FlashTag tag =
                base.ReadTag(reader, header);

            switch (tag.Header.TagType)
            {
                case FlashTagType.DoABC:
                _abcTags.Add((DoABCTag)tag);
                break;

                case FlashTagType.DefineBinaryData:
                _binTags.Add((DefineBinaryDataTag)tag);
                break;
            }
            return tag;
        }
    }
}