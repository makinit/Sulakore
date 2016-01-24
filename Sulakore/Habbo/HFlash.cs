using System.IO;
using System.Linq;
using System.Collections.Generic;

using Sulakore.Disassembler;
using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Tags;
using Sulakore.Disassembler.Records;
using Sulakore.Disassembler.ActionScript;
using Sulakore.Disassembler.ActionScript.Traits;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Habbo
{
    public class HFlash : ShockwaveFlash
    {
        private readonly IList<ABCFile> _abcFiles;
        private readonly IList<DefineBinaryDataTag> _binTags;

        /// <summary>
        /// Gets the original modulus.
        /// </summary>
        public string Modulus { get; private set; }
        /// <summary>
        /// Gets the original exponent.
        /// </summary>
        public string Exponent { get; private set; }

        public Dictionary<ushort, ASClass> OutgoingTypes { get; }
        public Dictionary<ushort, ASClass> IncomingTypes { get; }

        public HFlash(byte[] data)
            : base(data)
        {
            _abcFiles = new List<ABCFile>();
            _binTags = new List<DefineBinaryDataTag>();

            OutgoingTypes = new Dictionary<ushort, ASClass>();
            IncomingTypes = new Dictionary<ushort, ASClass>();
        }
        public HFlash(string path)
            : this(File.ReadAllBytes(path))
        {
            Location = Path.GetFullPath(path);
        }

        public bool FindMessageInstances()
        {
            ABCFile abc = _abcFiles[2];
            ASClass habboMessages = abc.FindClassByName("HabboMessages");
            if (habboMessages == null || habboMessages.Traits.Count < 2) return false;

            ASTrait incomingMap = habboMessages.Traits[0];
            ASTrait outgoingMap = habboMessages.Traits[1];

            using (var mapReader = new FlashReader(
                habboMessages.Constructor.Body.Bytecode.ToArray()))
            {
                while (mapReader.Position != mapReader.Length)
                {
                    OPCode op = mapReader.ReadOP();
                    if (op != OPCode.GetLex) continue;

                    int mapTypeIndex = mapReader.Read7BitEncodedInt();
                    bool isOutgoing = (mapTypeIndex == outgoingMap.TypeIndex);
                    bool isIncoming = (mapTypeIndex == incomingMap.TypeIndex);
                    if (!isOutgoing && !isIncoming) continue;

                    op = mapReader.ReadOP();
                    if (op != OPCode.PushShort && op != OPCode.PushByte) continue;

                    ushort header = 0;
                    if (op == OPCode.PushByte)
                    {
                        header = mapReader.ReadByte();
                    }
                    else header = (ushort)mapReader.Read7BitEncodedInt();

                    op = mapReader.ReadOP();
                    if (op != OPCode.GetLex) continue;

                    int messageTypeIndex = mapReader.Read7BitEncodedInt();
                    ASMultiname messageType = abc.Constants.Multinames[messageTypeIndex];
                    ASClass messageInstance = abc.FindClassByName(messageType.ObjName);

                    if (isOutgoing) OutgoingTypes[header] = messageInstance;
                    else if (isIncoming) IncomingTypes[header] = messageInstance;
                }
            }

            return (OutgoingTypes.Count > 0 &&
                IncomingTypes.Count > 0);
        }
        public bool InjectIncomingLogger()
        {
            return false;
        }
        public bool InjectOutgoingLogger()
        {
            return false;
        }

        public bool BypassRemoteHostCheck()
        {
            ABCFile abc = _abcFiles[2];
            ASInstance commManager = abc.FindInstanceByName("HabboCommunicationManager");
            if (commManager == null) return false;

            // The "host" value is always the first slot, for now.
            string hostValueSlotName = commManager.FindTraits<SlotConstantTrait>(TraitType.Slot)
                .Where(t => t.Type.ObjName == "String").ToArray()[0].ObjName;

            ASMethod initComponent = commManager.FindMethod("initComponent", "void").Method;
            if (initComponent == null) return false;

            using (var inCode = new FlashReader(initComponent.Body.Bytecode))
            using (var outCode = new FlashWriter(inCode.Length))
            {
                int hostSlotIndex = abc.Constants.IndexOfMultiname(hostValueSlotName);
                while (inCode.Position != inCode.Length)
                {
                    OPCode op = inCode.ReadOP();
                    outCode.WriteOP(op);
                    if (op != OPCode.GetLocal_0) continue;

                    op = inCode.ReadOP();
                    outCode.WriteOP(op);
                    if (op != OPCode.CallPropVoid) continue;

                    int callPropVoidIndex = inCode.Read7BitEncodedInt();
                    outCode.Write7BitEncodedInt(callPropVoidIndex);

                    int callPropVoidArgCount = inCode.Read7BitEncodedInt();
                    outCode.Write7BitEncodedInt(callPropVoidArgCount);

                    if (callPropVoidArgCount != 0) continue;

                    int getPropertyNameIndex = abc.Constants
                        .IndexOfMultiname("getProperty");

                    outCode.WriteOP(OPCode.GetLocal_0);
                    outCode.WriteOP(OPCode.FindPropStrict);
                    outCode.Write7BitEncodedInt(getPropertyNameIndex);

                    outCode.WriteOP(OPCode.PushString);
                    outCode.Write7BitEncodedInt(abc.Constants.AddString("connection.info.host"));

                    outCode.WriteOP(OPCode.CallProperty);
                    outCode.Write7BitEncodedInt(getPropertyNameIndex);
                    outCode.Write7BitEncodedInt(1);

                    outCode.WriteOP(OPCode.InitProperty);
                    outCode.Write7BitEncodedInt(hostSlotIndex);

                    outCode.Write(inCode.ToArray(),
                        inCode.Position, inCode.Length - inCode.Position);

                    do op = inCode.ReadOP();
                    while (op != OPCode.CallPropVoid);

                    callPropVoidIndex = inCode.Read7BitEncodedInt();
                    ASMultiname callPropVoidName = abc.Constants.Multinames[callPropVoidIndex];
                    ASMethod connectMethod = commManager.FindMethod(callPropVoidName.ObjName, "void").Method;
                    RemoveHostSuffix(abc, connectMethod);

                    initComponent.Body.Bytecode = outCode.ToArray();
                    return true;
                }
            }
            return false;
        }
        protected void RemoveHostSuffix(ABCFile abc, ASMethod connectMethod)
        {
            using (var inCode = new FlashReader(connectMethod.Body.Bytecode))
            using (var outCode = new FlashWriter(inCode.Length))
            {
                int ifNeCount = 0;
                while (inCode.Position != inCode.Length)
                {
                    OPCode op = inCode.ReadOP();
                    outCode.WriteOP(op);
                    if (op == OPCode.IfNe && ++ifNeCount == 2)
                    {
                        var iFNeJumpCount = (int)inCode.ReadS24();
                        outCode.WriteS24(iFNeJumpCount + 6);
                        continue;
                    }
                    else if (op != OPCode.PushInt) continue;

                    int pushIntIndex = inCode.Read7BitEncodedInt();
                    int integerValue = abc.Constants.Integers[pushIntIndex];
                    switch (integerValue)
                    {
                        case 65244:
                        case 65185:
                        case 65191:
                        case 65189:
                        case 65188:
                        case 65174:
                        case 65238:
                        case 65184:
                        case 65171:
                        case 65172:
                        {
                            pushIntIndex = abc.Constants.AddInteger(65290);
                            break;
                        }
                    }
                    outCode.Write7BitEncodedInt(pushIntIndex);
                }
                connectMethod.Body.Bytecode = outCode.ToArray();
            }
            RemoveDeadFalseConditions(connectMethod.Body);
        }

        public bool DisableClientEncryption()
        {
            ASInstance rc4 = _abcFiles[2].FindInstanceByName("ArcFour");
            if (rc4 == null) _abcFiles[2].FindInstanceByName("RC4");
            if (rc4 == null) return false;

            int modifyCount = 0;
            foreach (ASTrait trait in rc4.Traits)
            {
                if (trait.TraitType != TraitType.Method) continue;
                var rc4Method = ((MethodGetterSetterTrait)trait.Data).Method;

                if (rc4Method.ReturnType.ObjName != "ByteArray") continue;
                if (rc4Method.Parameters.Count != 1) continue;
                if (rc4Method.Parameters[0].Type.ObjName != "ByteArray") continue;

                modifyCount++;
                InsertEarlyReturnLocal(rc4Method, 1);
            }
            return (modifyCount >= 2);
        }
        public bool RemoveLocalUseRestrictions()
        {
            ASMethod unloadingMethod = _abcFiles[0].Classes[0]
                .FindMethod("*", "Boolean")?.Method;

            if (unloadingMethod == null)
                return false;

            ASClass habboClass = _abcFiles[1].FindClassByName("Habbo");
            if (habboClass == null) return false;

            ASMethod domainValidatorMethod =
                habboClass.FindMethod("*", "Boolean")?.Method;

            if (domainValidatorMethod == null)
            {
                domainValidatorMethod = habboClass.Instance
                    .FindMethod("*", "Boolean")?.Method;
            }

            if (domainValidatorMethod == null) return false;
            if (domainValidatorMethod.Parameters.Count == 0) return false;
            if (domainValidatorMethod.Parameters[0].Type.ObjName != "String") return false;

            InsertEarlyReturnTrue(domainValidatorMethod);
            InsertEarlyReturnTrue(unloadingMethod);
            return true;
        }
        public bool DisableExpirationDateCheck()
        {
            ABCFile abc = _abcFiles[2];
            ASInstance windowContext = abc.FindInstanceByName("WindowContext");
            if (windowContext == null) return false;

            using (var inCode = new FlashReader(windowContext.Constructor.Body.Bytecode))
            using (var outCode = new FlashWriter())
            {
                int setLocal11Itterations = 0;
                while (inCode.Position != inCode.Length)
                {
                    OPCode op = inCode.ReadOP();
                    outCode.WriteOP(op);
                    if (op != OPCode.SetLocal) continue;

                    int setLocalIndex = inCode.Read7BitEncodedInt();
                    outCode.Write7BitEncodedInt(setLocalIndex);
                    if (setLocalIndex != 11 || (++setLocal11Itterations != 2)) continue;

                    outCode.WriteOP(OPCode.ReturnVoid);
                    outCode.Write(inCode.ToArray(), inCode.Position,
                        inCode.Length - inCode.Position);

                    windowContext.Constructor.Body.Bytecode = outCode.ToArray();
                    return true;
                }
            }
            return false;
        }
        public bool ReplaceRSA(int exponent, string modulus)
        {
            ABCFile abc = _abcFiles[_abcFiles.Count - 1];
            int modulusIndex = abc.Constants.AddString(modulus);

            int exponentIndex = abc.Constants
                .AddString(exponent.ToString("x"));

            int rsaStart = 0;
            ASInstance commClass = abc.FindInstanceByName("HabboCommunicationDemo");
            ASMethod verifier = FindVerifyMethod(commClass, abc, out rsaStart);

            using (var inCode = new FlashReader(verifier.Body.Bytecode))
            using (var outCode = new FlashWriter(inCode.Length))
            {
                bool searchingKeys = true;
                inCode.Position = rsaStart;
                outCode.Write(inCode.ToArray(), 0, rsaStart);

                while (inCode.Position != inCode.Length)
                {
                    byte codeByte = inCode.ReadByte();
                    outCode.Write(codeByte);

                    if (!searchingKeys)
                    {
                        outCode.Write(inCode.ToArray(),
                            inCode.Position, inCode.Length - inCode.Position);

                        break;
                    }
                    switch ((OPCode)codeByte)
                    {
                        case OPCode.GetLex:
                        {
                            outCode.Position--;
                            outCode.WriteOP(OPCode.PushString);

                            int typeIndex = inCode.Read7BitEncodedInt();
                            ASMultiname type = abc.Constants.Multinames[typeIndex];

                            inCode.ReadOP();
                            inCode.Read7BitEncodedInt();
                            inCode.Read7BitEncodedInt();

                            if (modulusIndex > 0)
                            {
                                outCode.Write7BitEncodedInt(modulusIndex);
                                modulusIndex = -1;
                            }
                            else if (searchingKeys)
                            {
                                outCode.Write7BitEncodedInt(exponentIndex);
                                searchingKeys = false;
                            }
                            break;
                        }
                        case OPCode.PushString:
                        {
                            int stringIndex = inCode.Read7BitEncodedInt();
                            string value = abc.Constants.Strings[stringIndex];

                            if (string.IsNullOrWhiteSpace(Modulus))
                            {
                                Modulus = value;
                                outCode.Write7BitEncodedInt(modulusIndex);
                            }
                            else if (string.IsNullOrWhiteSpace(Exponent))
                            {
                                Exponent = value;
                                outCode.Write7BitEncodedInt(exponentIndex);

                                searchingKeys = false;
                            }
                            break;
                        }
                        default: continue;
                    }
                }

                verifier.Body.Bytecode = outCode.ToArray();
                if (!searchingKeys) return true;
            }
            return false;
        }

        protected void RemoveDeadFalseConditions(ASMethodBody body)
        {
            using (var inCode = new FlashReader(body.Bytecode))
            using (var outCode = new FlashWriter(inCode.Length))
            {
                while (inCode.Position != inCode.Length)
                {
                    OPCode op = inCode.ReadOP();
                    if (op != OPCode.PushFalse)
                    {
                        outCode.WriteOP(op);
                        continue;
                    }
                    op = inCode.ReadOP();
                    if (op != OPCode.PushFalse)
                    {
                        outCode.WriteOP(OPCode.PushFalse);
                        outCode.WriteOP(op);
                        continue;
                    }
                    op = inCode.ReadOP();
                    if (op != OPCode.IfNe)
                    {
                        outCode.WriteOP(OPCode.PushFalse);
                        outCode.WriteOP(OPCode.PushFalse);
                        outCode.WriteOP(op);
                        continue;
                    }
                    else inCode.ReadS24();
                }
                body.Bytecode = outCode.ToArray();
            }
        }
        protected void InsertEarlyReturnTrue(ASMethod method)
        {
            method.Body.Bytecode[0] = (byte)OPCode.PushTrue;
            method.Body.Bytecode[1] = (byte)OPCode.ReturnValue;
        }
        protected void InsertEarlyReturnLocal(ASMethod method, byte local)
        {
            method.Body.Bytecode[0] = (byte)(local + OPCode.GetLocal_0);
            method.Body.Bytecode[1] = (byte)OPCode.ReturnValue;
        }
        protected ASMethod FindVerifyMethod(ASInstance instance, ABCFile abc, out int rsaStart)
        {
            List<MethodGetterSetterTrait> methodTraits =
                instance.FindTraits<MethodGetterSetterTrait>(TraitType.Method);

            rsaStart = -1;
            foreach (MethodGetterSetterTrait mgsTrait in methodTraits)
            {
                ASMethod method = mgsTrait.Method;

                if (method.ReturnType.ObjName != "void") continue;
                if (method.Parameters.Count != 1) continue;

                using (var code = new FlashReader(method.Body.Bytecode))
                {
                    while (code.Position != code.Length)
                    {
                        OPCode op = code.ReadOP();
                        if (op != OPCode.GetLex) continue;

                        int typeIndex = code.Read7BitEncodedInt();
                        ASMultiname type = abc.Constants.Multinames[typeIndex];

                        if (type?.ObjName == "RSAKey")
                        {
                            rsaStart = code.Position;
                            return method;
                        }
                    }
                }
            }
            return null;
        }

        public string GetHash(ASClass asClass, bool isOutgoing)
        {
            return GetHash(GetHashData(asClass, isOutgoing));
        }
        protected virtual byte[] GetHashData(ASClass asClass, bool isOutgoing)
        {
            using (var hashStream = new MemoryStream())
            using (var hashInput = new BinaryWriter(hashStream))
            {
                hashInput.Write(GetHashData(asClass));
                if (isOutgoing)
                {
                    hashInput.Write("OUTGOING");
                    WriteOutgoingHashData(hashInput, asClass);
                }
                else
                {
                    hashInput.Write("INCOMING");
                    WriteIncomingHashData(hashInput, asClass);
                }
                return hashStream.ToArray();
            }
        }
        protected virtual void WriteIncomingHashData(BinaryWriter hashInput, ASClass incomingType)
        {
            ASClass parserType = null;
            using (var codeOutput = new FlashReader(
                incomingType.Instance.Constructor.Body.Bytecode))
            {
                while (codeOutput.IsDataAvailable)
                {
                    OPCode op = codeOutput.ReadOP();
                    object[] values = codeOutput.ReadValues(op);
                    if (op == OPCode.GetLex)
                    {
                        int getLexIndex = (int)values[0];

                        ASMultiname getLexMultiname = incomingType.ABC
                            .Constants.Multinames[getLexIndex];

                        parserType = incomingType.ABC
                            .FindClassByName(getLexMultiname.ObjName);

                        break;
                    }
                }
            }

            if (parserType != null)
                hashInput.Write(GetHashData(parserType));
            else
                WriteIncomingParserHashData(hashInput, incomingType);
        }
        protected virtual void WriteOutgoingHashData(BinaryWriter hashInput, ASClass outgoingType)
        {
            // If ALL Outgoing message types begin to get their names obfuscated,
            // remove this condition/check.
            string outgoingTypeName =
                outgoingType.Instance.Type.ObjName;

            if (outgoingTypeName.EndsWith("Composer"))
                hashInput.Write(outgoingTypeName);
        }
        protected virtual void WriteIncomingParserHashData(BinaryWriter hashInput, ASClass incomingType)
        {
            ASInstance eventSuperInstance = incomingType.ABC.FindInstanceByName(
                incomingType.Instance.SuperType.ObjName);

            ASMultiname parserReturnType = eventSuperInstance
                .FindGetter("parser").Method.ReturnType;

            SlotConstantTrait parserSlot = eventSuperInstance
                .FindSlot("*", parserReturnType.ObjName);

            foreach (ASTrait trait in incomingType.Instance.Traits)
            {
                if (trait.TraitType != TraitType.Method) continue;

                var mgs = (MethodGetterSetterTrait)trait.Data;
                if (mgs.Method.Parameters.Count != 0) continue;

                using (var codeOutput =
                    new FlashReader(mgs.Method.Body.Bytecode))
                {
                    while (codeOutput.IsDataAvailable)
                    {
                        OPCode op = codeOutput.ReadOP();
                        object[] values = codeOutput.ReadValues(op);

                        if (op == OPCode.GetLex)
                        {
                            ASMultiname getLexType = incomingType.ABC
                                .Constants.Multinames[(int)values[0]];

                            if (getLexType.ObjName == parserSlot.ObjName)
                            {
                                ASClass parserType = incomingType.ABC
                                    .FindClassByName(mgs.Method.ReturnType.ObjName);

                                hashInput.Write(GetHashData(parserType));
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override List<FlashTag> ReadTags()
        {
            _binTags.Clear();
            _abcFiles.Clear();

            OutgoingTypes.Clear();
            IncomingTypes.Clear();

            return base.ReadTags();
        }
        protected override FlashTag ReadTag(FlashReader reader, TagRecord header)
        {
            FlashTag tag =
                base.ReadTag(reader, header);

            switch (tag.Header.TagType)
            {
                case FlashTagType.DoABC:
                _abcFiles.Add(((DoABCTag)tag).ABC);
                break;

                case FlashTagType.DefineBinaryData:
                _binTags.Add((DefineBinaryDataTag)tag);
                break;
            }
            return tag;
        }
    }
}