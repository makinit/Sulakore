using System.Collections.Generic;

using FlashInspect.IO;

namespace FlashInspect.ActionScript
{
    public class ASMetadata
    {
        private readonly ABCFile _abc;

        public string Name
        {
            get { return _abc.Constants.Strings[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public Dictionary<int, int> Elements { get; }

        public ASMetadata(ABCFile abc)
        {
            _abc = abc;
            Elements = new Dictionary<int, int>();
        }
        public ASMetadata(ABCFile abc, int nameIndex) :
            this(abc)
        {
            NameIndex = nameIndex;
        }
        public ASMetadata(ABCFile abc, int nameIndex, IDictionary<int, int> elements) :
            this(abc, nameIndex)
        {
            foreach (KeyValuePair<int, int> element in Elements)
                Elements.Add(element.Key, element.Value);
        }

        public ASMetadata(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            NameIndex = reader.Read7BitEncodedInt();
            int itemInfoCount = reader.Read7BitEncodedInt();

            Elements = new Dictionary<int, int>(itemInfoCount);
            for (int i = 0; i < itemInfoCount; i++)
            {
                Elements.Add(reader.Read7BitEncodedInt(),
                    reader.Read7BitEncodedInt());
            }
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NameIndex);
                abc.Write7BitEncodedInt(Elements.Count);

                foreach (KeyValuePair<int, int> item in Elements)
                {
                    abc.Write7BitEncodedInt(item.Key);
                    abc.Write7BitEncodedInt(item.Value);
                }

                return abc.ToArray();
            }
        }
    }
}