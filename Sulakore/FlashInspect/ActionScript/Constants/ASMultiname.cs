using System;
using System.Diagnostics;

using FlashInspect.IO;
using FlashInspect.ActionScript.Multinames;

namespace FlashInspect.ActionScript.Constants
{
    [DebuggerDisplay("{ObjName} | MultinameType: {MultinameType}")]
    public class ASMultiname
    {
        private readonly ASConstants _constants;

        public int ObjNameIndex { get; internal set; }
        public string ObjName => _constants.Strings[ObjNameIndex];

        public IMultiname Data { get; set; }
        public ConstantType MultinameType => Data.MultinameType;

        public ASMultiname(ASConstants constants, FlashReader reader)
        {
            _constants = constants;

            var multinameType = (ConstantType)reader.ReadByte();
            switch (multinameType)
            {
                case ConstantType.QName:
                case ConstantType.QNameA:
                {
                    var qName = new QName(constants, reader, multinameType);
                    ObjNameIndex = qName.NameIndex;

                    Data = qName;
                    break;
                }

                case ConstantType.RTQName:
                case ConstantType.RTQNameA:
                {
                    var rtqName = new RTQName(constants, reader, multinameType);
                    ObjNameIndex = rtqName.NameIndex;

                    Data = rtqName;
                    break;
                }

                case ConstantType.RTQNameL:
                case ConstantType.RTQNameLA:
                Data = new RTQNameL(multinameType);
                break;

                case ConstantType.Multiname:
                case ConstantType.MultinameA:
                {
                    var multiname = new Multiname(constants, reader, multinameType);
                    ObjNameIndex = multiname.NameIndex;

                    Data = multiname;
                    break;
                }

                case ConstantType.MultinameL:
                case ConstantType.MultinameLA:
                Data = new MultinameL(constants, reader, multinameType);
                break;

                case ConstantType.Typename:
                Data = new Typename(constants, reader);
                break;

                default:
                throw new Exception("Invalid multiname: " + multinameType);
            }
        }

        public byte[] ToArray()
        {
            using (var abc = new FlashWriter())
            {
                abc.Write((byte)MultinameType);
                abc.Write(Data.ToArray());

                return abc.ToArray();
            }
        }
    }
}