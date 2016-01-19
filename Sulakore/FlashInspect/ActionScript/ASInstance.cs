using System.Diagnostics;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.ActionScript.Traits;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript
{
    [DebuggerDisplay("{Name.ObjName} | Traits: {Traits.Count}")]
    public class ASInstance : TraitContainer
    {
        private readonly ABCFile _abc;

        public List<int> InterfaceIndices { get; }
        public override List<ASTrait> Traits { get; }

        public ASMultiname Name
        {
            get { return _abc.Constants.Multinames[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public ASMultiname SuperName
        {
            get { return _abc.Constants.Multinames[SuperNameIndex]; }
        }
        public int SuperNameIndex { get; set; }

        public ASMethod Constructor
        {
            get { return _abc.Methods[ConstructorIndex]; }
        }
        public int ConstructorIndex { get; set; }

        public ASNamespace ProtectedNamespace
        {
            get { return _abc.Constants.Namespaces[ProtectedNamespaceIndex]; }
        }
        public int ProtectedNamespaceIndex { get; set; }

        public ClassFlags ClassInfo { get; set; }

        public ASInstance(ABCFile abc)
        {
            _abc = abc;
            Traits = new List<ASTrait>();
            InterfaceIndices = new List<int>();
        }
        public ASInstance(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            NameIndex = reader.Read7BitEncodedInt();
            SuperNameIndex = reader.Read7BitEncodedInt();
            ClassInfo = (ClassFlags)reader.ReadByte();

            if ((ClassInfo & ClassFlags.ProtectedNamespace) != 0)
                ProtectedNamespaceIndex = reader.Read7BitEncodedInt();

            InterfaceIndices = new List<int>(reader.Read7BitEncodedInt());
            for (int i = 0; i < InterfaceIndices.Capacity; i++)
                InterfaceIndices.Add(reader.Read7BitEncodedInt());

            ConstructorIndex = reader.Read7BitEncodedInt();
            Traits = new List<ASTrait>(reader.Read7BitEncodedInt());

            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write7BitEncodedInt(NameIndex);
                abc.Write7BitEncodedInt(SuperNameIndex);
                abc.Write7BitEncodedInt((byte)ClassInfo);

                if ((ClassInfo & ClassFlags.ProtectedNamespace) != 0)
                    abc.Write7BitEncodedInt(ProtectedNamespaceIndex);

                abc.Write7BitEncodedInt(InterfaceIndices.Count);
                foreach (int index in InterfaceIndices)
                    abc.Write7BitEncodedInt(index);

                abc.Write7BitEncodedInt(ConstructorIndex);
                abc.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    abc.Write(trait.ToArray());

                return abc.ToArray();
            }
        }
    }
}