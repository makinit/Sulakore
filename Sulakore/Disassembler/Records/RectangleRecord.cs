using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.Records
{
    public class RectangleRecord
    {
        /// <summary>
        /// Gets the x-coordinate position for the start of the frame.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Gets the y-coordinate position for the start of the frame.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the width of the frame.
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Gets the height of the frame.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the width of the frame in twips(Width * 20).
        /// </summary>
        public int TwipsWidth { get; }
        /// <summary>
        /// Gets the height of the frame in twips(Height * 20).
        /// </summary>
        public int TwipsHeight { get; }

        public RectangleRecord(FlashReader reader)
        {
            int bits = reader.ReadUB(5);

            X = reader.ReadSB(bits);
            TwipsWidth = reader.ReadSB(bits);
            Width = (TwipsWidth / 20);

            Y = reader.ReadSB(bits);
            TwipsHeight = reader.ReadSB(bits);
            Height = (TwipsHeight / 20);
        }
    }
}