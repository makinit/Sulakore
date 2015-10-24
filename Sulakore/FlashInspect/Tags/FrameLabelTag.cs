using System.Text;

using FlashInspect.IO;
using FlashInspect.Records;

namespace FlashInspect.Tags
{
    /// <summary>
    /// Represents the name of a frame in a flash file.
    /// </summary>
    public class FrameLabelTag : FlashTag
    {
        /// <summary>
        /// Gets or sets the name of the frame.
        /// </summary>
        public string Name { get; set; }

        public FrameLabelTag() :
            this(string.Empty)
        { }
        public FrameLabelTag(string name) :
            base(FlashTagType.FrameLabel)
        {
            Name = name;
        }

        public FrameLabelTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            Name = reader.ReadNullTerminatedString();
        }

        protected override byte[] OnConstruct()
        {
            return Encoding.UTF8.GetBytes(Name + "\0");
        }
    }
}