using System;
using System.IO;

using Sulakore.Compression.SevenZip.Compression.LZMA;

namespace Sulakore.Compression.SevenZip
{
    public static class LZMA
    {
        private static readonly CoderPropID[] _defaultIds;
        private static readonly object[] _defaultProperties;

        static LZMA()
        {
            _defaultIds = new CoderPropID[]
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            };
            _defaultProperties = new object[]
            {
                (1 << 23),
                2,
                3,
                0,
                2,
                32,
                "bt4",
                false
            };
        }

        public static byte[] CompressBuffer(byte[] data)
        {
            var lzmaEncoder = new LZMAEncoder();
            lzmaEncoder.SetCoderProperties(_defaultIds, _defaultProperties);

            using (var inStream = new MemoryStream(data))
            using (var outStream = new MemoryStream())
            {
                outStream.Position += 4;
                lzmaEncoder.WriteCoderProperties(outStream);
                lzmaEncoder.Code(inStream, outStream, -1, -1, null);

                byte[] compressed = outStream.ToArray();
                byte[] compressedLengthData = BitConverter.GetBytes(compressed.Length);
                System.Buffer.BlockCopy(compressedLengthData, 0, compressed, 0, 4);

                return compressed;
            }
        }
        public static byte[] DecompressBuffer(byte[] data, int bufferSize)
        {
            var lzmaDecoder = new LZMADecoder();
            using (var inStream = new MemoryStream(data))
            using (var outStream = new MemoryStream(bufferSize))
            {
                inStream.Position += 4;

                var lzmaProperties = new byte[5];
                inStream.Read(lzmaProperties, 0, 5);

                lzmaDecoder.SetDecoderProperties(lzmaProperties);
                lzmaDecoder.Code(inStream, outStream, inStream.Length, bufferSize, null);
                return outStream.ToArray();
            }
        }
    }
}