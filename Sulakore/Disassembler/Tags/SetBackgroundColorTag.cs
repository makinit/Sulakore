using System.Drawing;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
{
    /// <summary>
    /// Represents the color to use for the display's background.
    /// </summary>
    public class SetBackgroundColorTag : FlashTag
    {
        /// <summary>
        /// Gets or sets the color of the display background
        /// </summary>
        public Color BackgroundColor { get; set; }

        public SetBackgroundColorTag() :
            this(Color.Black)
        { }
        public SetBackgroundColorTag(Color backgroundColor) :
            base(FlashTagType.SetBackgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public SetBackgroundColorTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            byte red = reader.ReadByte();
            byte green = reader.ReadByte();
            byte blue = reader.ReadByte();

            BackgroundColor =
                Color.FromArgb(red, green, blue);
        }

        protected override byte[] OnConstruct()
        {
            var buffer = new byte[3];
            buffer[0] = BackgroundColor.R;
            buffer[1] = BackgroundColor.G;
            buffer[2] = BackgroundColor.B;

            return buffer;
        }
    }
}