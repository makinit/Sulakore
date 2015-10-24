using System.Collections.Generic;

using FlashInspect.IO;

namespace FlashInspect.ActionScript.Constants
{
    public class ASNamespaceSet
    {
        private readonly ASConstants _constants;

        public List<int> NamespaceIndices { get; }

        public ASNamespaceSet(ASConstants constants)
        {
            NamespaceIndices = new List<int>();
        }
        public ASNamespaceSet(ASConstants constants, IList<int> namespaceIndices) :
            this(constants)
        {
            NamespaceIndices.AddRange(namespaceIndices);
        }

        public ASNamespaceSet(ASConstants constants, FlashReader reader)
        {
            _constants = constants;

            NamespaceIndices =
                new List<int>(reader.Read7BitEncodedInt());

            for (int i = 0; i < NamespaceIndices.Capacity; i++)
                NamespaceIndices.Add(reader.Read7BitEncodedInt());
        }

        public IList<ASNamespace> GetNamespaces()
        {
            var namespaces = new List<ASNamespace>(NamespaceIndices.Count);
            foreach (int namespaceIndex in NamespaceIndices)
            {
                namespaces.Add(
                    _constants.Namespaces[namespaceIndex]);
            }
            return namespaces;
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NamespaceIndices.Count);

                foreach (int namespaceIndex in NamespaceIndices)
                    abc.Write7BitEncodedInt(namespaceIndex);

                return abc.ToArray();
            }
        }
    }
}