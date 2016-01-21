using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
{
    /// <summary>
    /// Includes two properties that can be used to override the default settings for maximum recursion depth and ActionScript time-out.
    /// </summary>
    public class ScriptLimitsTag : FlashTag
    {
        /// <summary>
        /// Gets or sets the maximum recursion depth of the associated flash file.
        /// </summary>
        public ushort MaxRecursionDepth { get; set; }
        /// <summary>
        /// Gets or sets the maximum ActionScript processing time before a "script stuck" dialog box is displayed.
        /// </summary>
        public ushort ScriptTimeoutSeconds { get; set; }

        public ScriptLimitsTag() :
            this(0, 0)
        { }
        public ScriptLimitsTag(ushort maxRecursionDepth, ushort scriptTimeoutSeconds) :
            base(FlashTagType.ScriptLimits)
        {
            MaxRecursionDepth = maxRecursionDepth;
            ScriptTimeoutSeconds = scriptTimeoutSeconds;
        }

        public ScriptLimitsTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            MaxRecursionDepth = reader.ReadUInt16();
            ScriptTimeoutSeconds = reader.ReadUInt16();
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter(4))
            {
                tag.Write(MaxRecursionDepth);
                tag.Write(ScriptTimeoutSeconds);

                return tag.ToArray();
            }
        }
    }
}