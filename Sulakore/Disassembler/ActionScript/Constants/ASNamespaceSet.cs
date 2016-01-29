using System.Collections.Generic;
using System.Diagnostics;
using Sulakore.Disassembler.IO;

namespace Sulakore.Disassembler.ActionScript.Constants
{
    [DebuggerDisplay("NamespaceIndices: {NamespaceIndices.Count}")]
    public class ASNamespaceSet : IConstant
    {
        public ABCFile ABC { get; }
        public List<int> NamespaceIndices { get; }

        public int ConstantIndex { get; internal set; }

        public ASNamespaceSet(ABCFile abc)
        {
            ABC = abc;
            NamespaceIndices = new List<int>();
        }
        public ASNamespaceSet(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            NamespaceIndices.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < NamespaceIndices.Capacity; i++)
                NamespaceIndices.Add(reader.Read7BitEncodedInt());
        }

        public IList<ASNamespace> GetNamespaces()
        {
            var namespaces = new List<ASNamespace>(NamespaceIndices.Count);
            foreach (int namespaceIndex in NamespaceIndices)
            {
                namespaces.Add(ABC.Constants
                    .Namespaces[namespaceIndex]);
            }
            return namespaces;
        }

        public byte[] ToByteArray()
        {
            using (var asNamespaceSet = new FlashWriter())
            {
                asNamespaceSet.Write7BitEncodedInt(NamespaceIndices.Count);

                foreach (int namespaceIndex in NamespaceIndices)
                    asNamespaceSet.Write7BitEncodedInt(namespaceIndex);

                return asNamespaceSet.ToArray();
            }
        }
    }
}