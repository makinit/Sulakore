﻿using System;
using System.Diagnostics;
using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Traits;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler.ActionScript
{
    [DebuggerDisplay("{Data?.ObjName}, MetadataIndices: {MetadataIndices.Count}, Attributes: {Attributes}, TraitType: {TraitType}")]
    public class ASTrait : ITrait
    {
        public int Id => Data.Id;
        public ABCFile ABC { get; }
        public string ObjName => Data.ObjName;
        public List<int> MetadataIndices { get; }
        public TraitType TraitType => Data.TraitType;

        public ITrait Data { get; set; }
        public TraitAttributes Attributes { get; set; }

        public ASMultiname Type
        {
            get { return ABC.Constants.Multinames[TypeIndex]; }
        }
        public int TypeIndex { get; set; }

        public ASTrait(ABCFile abc)
        {
            ABC = abc;
            MetadataIndices = new List<int>();
        }
        public ASTrait(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            TypeIndex = reader.Read7BitEncodedInt();
            byte trueKind = reader.ReadByte();

            var traitType = (TraitType)(trueKind & 0xF);
            Attributes = (TraitAttributes)(trueKind >> 4);
            #region Trait Reading
            switch (traitType)
            {
                case TraitType.Slot:
                case TraitType.Constant:
                {
                    Data = new SlotConstantTrait(abc, reader, traitType)
                    { ObjName = Type.ObjName };
                    break;
                }
                case TraitType.Method:
                case TraitType.Getter:
                case TraitType.Setter:
                {
                    Data = new MethodGetterSetterTrait(abc, reader, traitType)
                    { ObjName = Type.ObjName };

                    ((MethodGetterSetterTrait)Data).Method.ObjName = ObjName;
                    break;
                }
                case TraitType.Class:
                {
                    Data = new ClassTrait(abc, reader)
                    { ObjName = Type.ObjName };
                    break;
                }
                case TraitType.Function:
                {
                    Data = new FunctionTrait(abc, reader)
                    { ObjName = Type.ObjName };
                    break;
                }

                default:
                throw new Exception($"Invalid {nameof(ASTrait)} type: " + traitType);
            }
            #endregion

            if ((Attributes & TraitAttributes.Metadata) != 0)
                MetadataIndices.Capacity = reader.Read7BitEncodedInt();

            for (int i = 0; i < MetadataIndices.Capacity; i++)
                MetadataIndices.Add(reader.Read7BitEncodedInt());
        }

        public byte[] ToByteArray()
        {
            using (var asTrait = new FlashWriter())
            {
                var trueKind = (byte)((
                    (byte)Attributes << 4) + (byte)TraitType);

                asTrait.Write7BitEncodedInt(TypeIndex);
                asTrait.Write(trueKind);
                asTrait.Write(Data.ToByteArray());

                if ((Attributes & TraitAttributes.Metadata) != 0)
                {
                    asTrait.Write7BitEncodedInt(MetadataIndices.Count);

                    foreach (int index in MetadataIndices)
                        asTrait.Write7BitEncodedInt(index);
                }
                return asTrait.ToArray();
            }
        }
    }
}