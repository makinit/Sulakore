using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Traits;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{Type.ObjName}, SuperType: {SuperType?.ObjName}, Traits: {Traits.Count}, Info: {ClassInfo}")]
    public class ASInstance : TraitContainer, IABCChild
    {
        public ABCFile ABC { get; }
        public List<int> InterfaceIndices { get; }
        public override List<ASTrait> Traits { get; }

        public ASMultiname Type
        {
            get { return ABC.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public ASMultiname SuperType
        {
            get { return ABC.Constants.Multinames[SuperTypeIndex]; }
        }
        public int SuperTypeIndex { get; set; }

        public ASMethod Constructor
        {
            get { return ABC.Methods[ConstructorIndex]; }
        }
        public int ConstructorIndex { get; set; }

        public ASNamespace ProtectedNamespace
        {
            get { return ABC.Constants.Namespaces[ProtectedNamespaceIndex]; }
        }
        public int ProtectedNamespaceIndex { get; set; }

        public ClassFlags ClassInfo { get; set; }

        public ASInstance(ABCFile abc)
        {
            ABC = abc;
            Traits = new List<ASTrait>();
            InterfaceIndices = new List<int>();
        }
        public ASInstance(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            TypeIndex = reader.Read7BitEncodedInt();
            SuperTypeIndex = reader.Read7BitEncodedInt();
            ClassInfo = (ClassFlags)reader.ReadByte();

            if ((ClassInfo & ClassFlags.ProtectedNamespace) != 0)
                ProtectedNamespaceIndex = reader.Read7BitEncodedInt();

            InterfaceIndices.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < InterfaceIndices.Capacity; i++)
                InterfaceIndices.Add(reader.Read7BitEncodedInt());

            ConstructorIndex = reader.Read7BitEncodedInt();
            if (Constructor != null) Constructor.IsConstructor = true;

            Traits.Capacity = reader.Read7BitEncodedInt();
            for (int i = 0; i < Traits.Capacity; i++)
                Traits.Add(new ASTrait(abc, reader));
        }

        public byte[] ToByteArray()
        {
            using (var asInstance = new FlashWriter())
            {
                asInstance.Write7BitEncodedInt(TypeIndex);
                asInstance.Write7BitEncodedInt(SuperTypeIndex);
                asInstance.Write7BitEncodedInt((byte)ClassInfo);

                if ((ClassInfo & ClassFlags.ProtectedNamespace) != 0)
                    asInstance.Write7BitEncodedInt(ProtectedNamespaceIndex);

                asInstance.Write7BitEncodedInt(InterfaceIndices.Count);
                foreach (int index in InterfaceIndices)
                    asInstance.Write7BitEncodedInt(index);

                asInstance.Write7BitEncodedInt(ConstructorIndex);
                asInstance.Write7BitEncodedInt(Traits.Count);

                foreach (ASTrait trait in Traits)
                    asInstance.Write(trait.ToByteArray());

                return asInstance.ToArray();
            }
        }
    }
}