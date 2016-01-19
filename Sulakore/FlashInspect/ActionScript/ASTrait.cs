using System;
using System.Diagnostics;
using System.Collections.Generic;

using FlashInspect.IO;
using FlashInspect.ActionScript.Traits;
using FlashInspect.ActionScript.Constants;

namespace FlashInspect.ActionScript
{
    [DebuggerDisplay("{TraitType} | Name: {Name.ObjName}")]
    public class ASTrait
    {
        private readonly ABCFile _abc;

        public ITrait Data { get; set; }
        public TraitType TraitType => Data.TraitType;

        public ASMultiname Name
        {
            get { return _abc.Constants.Multinames[NameIndex]; }
        }
        public int NameIndex { get; set; }

        public List<int> MetadataIndices { get; }
        public TraitAttributes Attributes { get; set; }

        public ASTrait(ABCFile abc)
        {
            _abc = abc;
            MetadataIndices = new List<int>();
        }
        public ASTrait(ABCFile abc, int nameIndex) :
            this(abc)
        {
            NameIndex = nameIndex;
        }
        public ASTrait(ABCFile abc, int nameIndex, TraitAttributes attributes) :
            this(abc, nameIndex)
        {
            Attributes = attributes;
        }
        public ASTrait(ABCFile abc, int nameIndex, TraitAttributes attributes, IList<int> metadataIndices) :
            this(abc, nameIndex, attributes)
        {
            MetadataIndices.AddRange(metadataIndices);
        }

        public ASTrait(ABCFile abc, FlashReader reader)
        {
            _abc = abc;

            NameIndex = reader.Read7BitEncodedInt();
            byte trueKind = reader.ReadByte();

            var traitType = (TraitType)(trueKind & 0xF);
            Attributes = (TraitAttributes)(trueKind >> 4);
            #region Trait Reading
            switch (traitType)
            {
                case TraitType.Slot:
                case TraitType.Constant:
                Data = new SlotConstantTrait(abc, reader, Name.ObjName, traitType);
                break;

                case TraitType.Method:
                case TraitType.Getter:
                case TraitType.Setter:
                {
                    var mgsTrait = new MethodGetterSetterTrait(abc, reader, Name.ObjName, traitType);
                    mgsTrait.Method.ObjName = Name.ObjName;

                    Data = mgsTrait;
                    break;
                }

                case TraitType.Class:
                {
                    var classTrait = new ClassTrait(abc, reader, Name.ObjName);
                    // TODO: Link trait information?
                    Data = classTrait;
                    break;
                }

                case TraitType.Function:
                {
                    var functionTrait = new FunctionTrait(abc, reader, Name.ObjName);
                    // TODO: Link trait information?
                    Data = functionTrait;
                    break;
                }

                default:
                throw new Exception("Invalid trait: " + TraitType);
            }
            #endregion

            MetadataIndices = new List<int>();
            if ((Attributes & TraitAttributes.Metadata) != 0)
                MetadataIndices.Capacity = reader.Read7BitEncodedInt();

            for (int i = 0; i < MetadataIndices.Capacity; i++)
                MetadataIndices.Add(reader.Read7BitEncodedInt());
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                var trueKind = ((byte)Attributes << 4) + (byte)TraitType;

                abc.Write7BitEncodedInt(NameIndex);
                abc.Write((byte)trueKind);
                abc.Write(Data.ToArray());

                if ((Attributes & TraitAttributes.Metadata) != 0)
                {
                    abc.Write7BitEncodedInt(MetadataIndices.Count);

                    foreach (int index in MetadataIndices)
                        abc.Write7BitEncodedInt(index);
                }

                return abc.ToArray();
            }
        }
    }
}