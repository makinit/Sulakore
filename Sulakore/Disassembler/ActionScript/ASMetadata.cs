using System;
using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{ObjName}, Elements: {Elements.Count}")]
    public class ASMetadata : IABCChild
    {
        public ABCFile ABC { get; }
        public List<Tuple<int, int>> Elements { get; }

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; set; }

        public ASMetadata(ABCFile abc)
        {
            ABC = abc;
            Elements = new List<Tuple<int, int>>();
        }
        public ASMetadata(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            ObjNameIndex = reader.Read7BitEncodedInt();
            Elements.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < Elements.Capacity; i++)
            {
                int elementKey = reader.Read7BitEncodedInt();
                int elementValue = reader.Read7BitEncodedInt();
                Elements.Add(new Tuple<int, int>(elementKey, elementValue));
            }
        }

        public List<Tuple<string, string>> GetElements()
        {
            var elements = new List<Tuple<string, string>>(Elements.Capacity);
            foreach (Tuple<int, int> element in Elements)
            {
                string elementKey = ABC.Constants.Strings[element.Item1];
                string elementValue = ABC.Constants.Strings[element.Item2];
                elements.Add(new Tuple<string, string>(elementKey, elementValue));
            }
            return elements;
        }

        public byte[] ToByteArray()
        {
            using (var asMetadata = new FlashWriter())
            {
                asMetadata.Write7BitEncodedInt(ObjNameIndex);
                asMetadata.Write7BitEncodedInt(Elements.Count);
                foreach (Tuple<int, int> element in Elements)
                {
                    asMetadata.Write7BitEncodedInt(element.Item1);
                    asMetadata.Write7BitEncodedInt(element.Item2);
                }
                return asMetadata.ToArray();
            }
        }
    }
}