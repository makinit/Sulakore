using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{ObjName}, Elements: {Elements.Count}")]
    public class ASMetadata : IABCChild
    {
        public ABCFile ABC { get; }
        public Dictionary<int, int> Elements { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public ASMetadata(ABCFile abc)
        {
            ABC = abc;
            Elements = new Dictionary<int, int>();
        }
        public ASMetadata(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            ObjNameIndex = reader.Read7BitEncodedInt();
            int itemInfoCount = reader.Read7BitEncodedInt();

            for (int i = 0; i < itemInfoCount; i++)
            {
                Elements.Add(reader.Read7BitEncodedInt(),
                    reader.Read7BitEncodedInt());
            }
        }

        public byte[] ToByteArray()
        {
            using (var asMetadata = new FlashWriter())
            {
                asMetadata.Write7BitEncodedInt(ObjNameIndex);
                asMetadata.Write7BitEncodedInt(Elements.Count);
                foreach (KeyValuePair<int, int> item in Elements)
                {
                    asMetadata.Write7BitEncodedInt(item.Key);
                    asMetadata.Write7BitEncodedInt(item.Value);
                }
                return asMetadata.ToArray();
            }
        }
    }
}