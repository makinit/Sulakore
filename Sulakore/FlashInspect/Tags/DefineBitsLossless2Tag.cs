using System.Drawing;
using System.Drawing.Imaging;

using FlashInspect.IO;
using FlashInspect.Records;
using FlashInspect.Dictionary;

using Ionic.Zlib;

namespace FlashInspect.Tags
{
    /// <summary>
    /// Represents a lossless bitmap that contains ARGB bitmap data data compressed using the zlib format.
    /// </summary>
    public class DefineBitsLossless2Tag : FlashTag, ICharacter
    {
        private bool _isCompressed;
        private byte[] _bitmapData, _compressedBitmapData;

        /// <summary>
        /// Gets or sets the format of the compressed bitmap data.
        /// </summary>
        public byte BitmapFormat { get; set; }
        /// <summary>
        /// Gets the character id that represents the key in the global dictionary.
        /// </summary>
        public ushort CharacterId { get; set; }
        /// <summary>
        /// Gets or sets the width of the bitmap.
        /// </summary>
        public ushort BitmapWidth { get; set; }
        /// <summary>
        /// Gets or sets the height of the bitmap.
        /// </summary>
        public ushort BitmapHeight { get; set; }
        /// <summary>
        /// Gets or sets the number of colors in the bitmap, if <see cref="BitmapFormat"/> is an 8-bit colormapped image(3).
        /// </summary>
        public byte BitmapColorTableSize { get; set; }

        public DefineBitsLossless2Tag(Bitmap asset) :
            this(0, 5, asset)
        { }
        public DefineBitsLossless2Tag(ushort characterId, Bitmap asset) :
            this(characterId, 5, asset)
        { }
        public DefineBitsLossless2Tag(ushort characterId, byte bitmapFormat, Bitmap asset) :
            base(FlashTagType.DefineBitsLossless2)
        {
            CharacterId = characterId;
            BitmapFormat = bitmapFormat;

            if (asset != null)
                SetBitmap(asset);
        }

        public DefineBitsLossless2Tag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            CharacterId = reader.ReadUInt16();
            BitmapFormat = reader.ReadByte();
            BitmapWidth = reader.ReadUInt16();
            BitmapHeight = reader.ReadUInt16();

            _isCompressed = true;
            switch (BitmapFormat - 3)
            {
                case 0: break;

                case 1:
                case 2:
                {
                    _compressedBitmapData =
                        reader.ReadBytes(header.Body.Length - 7);

                    break;
                }
            }
        }

        /// <summary>
        /// Returns the bitmap located in the tag in an asynchronous operation.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            DecompressBitmapData();
            var bitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format32bppArgb);
            for (int i = 0, y = 0; i < _bitmapData.Length; i += 4)
            {
                int x = (i / 4) - (BitmapWidth * y);
                byte a = _bitmapData[i];
                byte r = _bitmapData[i + 1];
                byte g = _bitmapData[i + 2];
                byte b = _bitmapData[i + 3];

                bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                if (x == (BitmapWidth - 1)) y++;
            }
            return bitmap;
        }
        /// <summary>
        /// Replaces the bitmap in the tag, with the specified bitmap.
        /// </summary>
        /// <param name="asset">The bitmap to place in the tag.</param>
        public void SetBitmap(Bitmap asset)
        {
            int argbByteCount = ((asset.Width * asset.Height) * 4);
            argbByteCount += argbByteCount % 32;

            int position = 0;
            _bitmapData = new byte[argbByteCount];
            for (int y = 0; y < asset.Height; y++)
            {
                for (int x = 0; x < asset.Width; x++)
                {
                    Color pixel = asset.GetPixel(x, y);
                    _bitmapData[position++] = pixel.A;
                    _bitmapData[position++] = pixel.R;
                    _bitmapData[position++] = pixel.G;
                    _bitmapData[position++] = pixel.B;
                }
            }

            _isCompressed = false;
            _compressedBitmapData = null;

            BitmapWidth = (ushort)asset.Width;
            BitmapHeight = (ushort)asset.Height;
        }

        private void DecompressBitmapData()
        {
            if (_isCompressed)
            {
                _isCompressed = false;

                _bitmapData =
                    ZlibStream.UncompressBuffer(_compressedBitmapData);
            }
        }

        protected override byte[] OnConstruct()
        {
            if (_compressedBitmapData == null)
            {
                _compressedBitmapData =
                    ZlibStream.CompressBuffer(_bitmapData);
            }

            using (var tag =
                new FlashWriter(7 + _compressedBitmapData.Length))
            {
                tag.Write(CharacterId);
                tag.Write(BitmapFormat);
                tag.Write(BitmapWidth);
                tag.Write(BitmapHeight);
                tag.Write(_compressedBitmapData);

                return tag.ToArray();
            }
        }
    }
}