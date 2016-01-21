using System;
using System.Diagnostics;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript.Multinames;

namespace Sulakore.Disassembler.ActionScript.Constants
{
    [DebuggerDisplay("{ObjName}, MultinameType: {MultinameType}")]
    public class ASMultiname : IABCChild
    {
        public ABCFile ABC { get; }
        public ConstantType MultinameType => Data.MultinameType;

        public string ObjName
        {
            get { return ABC.Constants.Strings[ObjNameIndex]; }
        }
        public int ObjNameIndex { get; internal set; }

        public IMultiname Data { get; set; }
        
        public ASMultiname(ABCFile abc)
        {
            ABC = abc;
        }
        public ASMultiname(ABCFile abc, FlashReader reader)
            : this(abc)
        {
            var multinameType = (ConstantType)reader.ReadByte();
            switch (multinameType)
            {
                case ConstantType.QName:
                case ConstantType.QNameA:
                {
                    var qName = new QName(abc, reader, multinameType);
                    ObjNameIndex = qName.ObjNameIndex;
                    Data = qName;
                    break;
                }

                case ConstantType.RTQName:
                case ConstantType.RTQNameA:
                {
                    var rtqName = new RTQName(abc, reader, multinameType);
                    ObjNameIndex = rtqName.ObjNameIndex;
                    Data = rtqName;
                    break;
                }

                case ConstantType.RTQNameL:
                case ConstantType.RTQNameLA:
                Data = new RTQNameL(abc, multinameType);
                break;

                case ConstantType.Multiname:
                case ConstantType.MultinameA:
                {
                    var multiname = new Multiname(abc, reader, multinameType);
                    ObjNameIndex = multiname.ObjNameIndex;
                    Data = multiname;
                    break;
                }

                case ConstantType.MultinameL:
                case ConstantType.MultinameLA:
                Data = new MultinameL(abc, reader, multinameType);
                break;

                case ConstantType.Typename:
                Data = new Typename(abc, reader);
                break;

                default:
                throw new Exception("Invalid multiname: " + multinameType);
            }
        }

        public byte[] ToByteArray()
        {
            using (var asMultiname = new FlashWriter())
            {
                asMultiname.Write((byte)MultinameType);
                asMultiname.Write(Data.ToByteArray());
                return asMultiname.ToArray();
            }
        }
    }
}