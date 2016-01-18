using System.IO;
using System.Linq;
using System.Collections.Generic;

using FlashInspect;
using FlashInspect.IO;
using FlashInspect.Tags;
using FlashInspect.Records;
using FlashInspect.ActionScript;
using FlashInspect.ActionScript.Traits;
using FlashInspect.ActionScript.Constants;

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

        public Dictionary<ushort, ASInstance> OutgoingTypes { get; }
        public Dictionary<ushort, ASInstance> IncomingTypes { get; }

        public HFlash(byte[] data)
            : base(data)
        {
            _abcFiles = new List<ABCFile>();
            _binTags = new List<DefineBinaryDataTag>();

            OutgoingTypes = new Dictionary<ushort, ASInstance>();
            IncomingTypes = new Dictionary<ushort, ASInstance>();
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
                habboMessages.Constructor.Body.Code.ToArray()))
            {
                while (mapReader.Position != mapReader.Length)
                {
                    OPCode op = mapReader.ReadOP();
                    if (op != OPCode.GetLex) continue;

                    int mapTypeIndex = mapReader.Read7BitEncodedInt();
                    bool isOutgoing = (mapTypeIndex == outgoingMap.NameIndex);
                    bool isIncoming = (mapTypeIndex == incomingMap.NameIndex);
                    if (!isOutgoing && !isIncoming) continue;

                    op = mapReader.ReadOP();
                    if (op != OPCode.PushShort && op != OPCode.PushByte) continue;

                    var header = (ushort)mapReader
                        .Read7BitEncodedInt();

                    op = mapReader.ReadOP();
                    if (op != OPCode.GetLex) continue;

                    int messageTypeIndex = mapReader.Read7BitEncodedInt();
                    ASMultiname messageType = abc.Constants.Multinames[messageTypeIndex];
                    ASInstance messageInstance = abc.FindInstanceByName(messageType.ObjName);

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
            ABCFile abc = _abcFiles[2];

            ASInstance evaWireFormat = abc.FindInstanceByName("EvaWireFormat");
            if (evaWireFormat == null) return false;

            ASMethod encodeMethod = evaWireFormat.FindMethod("encode", "ByteArray")?.Method;
            if (encodeMethod == null) return false;

            string logFuncName = "OutgoingLog";
            int outLogTitleIndex = abc.Constants.Strings.IndexOf(logFuncName);
            if (outLogTitleIndex == -1)
            {
                abc.Constants.Strings.Add(logFuncName);
                outLogTitleIndex = (abc.Constants.Strings.Count - 1);
            }

            int callNameIndex = 0;
            int externalInterfaceIndex = 0;
            for (int i = 1; i < abc.Constants.Multinames.Count; i++)
            {
                ASMultiname multiname = abc.Constants.Multinames[i];
                switch (multiname.ObjName)
                {
                    case "call":
                    callNameIndex = i;
                    break;

                    case "ExternalInterface":
                    externalInterfaceIndex = i;
                    break;
                }

                if (callNameIndex != 0 &&
                    externalInterfaceIndex != 0)
                    break;
            }

            ASCode encodeCode = encodeMethod.Body.Code;
            int pushScopeIndex = encodeCode.IndexOf((byte)OPCode.PushScope);

            encodeMethod.Body.MaxStack = 4;
            encodeMethod.Body.LocalCount = 9;
            encodeMethod.Body.InitialScopeDepth = 9;
            encodeMethod.Body.MaxScopeDepth = 10;

            using (var outCode = new FlashWriter())
            {
                outCode.WriteOP(OPCode.GetLex);
                outCode.Write7BitEncodedInt(externalInterfaceIndex);

                // "OutgoingLog"
                outCode.WriteOP(OPCode.PushString);
                outCode.Write7BitEncodedInt(outLogTitleIndex);

                // int(param1) - Header
                outCode.WriteOP(OPCode.GetLocal_1);

                // Array(param2) - Objects
                outCode.WriteOP(OPCode.GetLocal_2);

                outCode.WriteOP(OPCode.CallPropVoid);
                outCode.Write7BitEncodedInt(callNameIndex);
                outCode.Write7BitEncodedInt(3);

                encodeCode.InsertRange(6, outCode.ToArray());
            }
            return true;
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

            ASCode initCode = initComponent.Body.Code;
            using (var inCode = new FlashReader(initCode.ToArray()))
            using (var outCode = new FlashWriter(inCode.Length))
            {
                int hostSlotIndex = abc.Constants.FindMultinameIndex(hostValueSlotName);
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
                        .FindMultinameIndex("getProperty");

                    outCode.WriteOP(OPCode.GetLocal_0);
                    outCode.WriteOP(OPCode.FindPropStrict);
                    outCode.Write7BitEncodedInt(getPropertyNameIndex);

                    outCode.WriteOP(OPCode.PushString);
                    outCode.Write7BitEncodedInt(abc.Constants.PushString("connection.info.host"));

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

                    initCode.Clear();
                    initCode.AddRange(outCode.ToArray());
                    return true;
                }
            }
            return false;
        }
        protected void RemoveHostSuffix(ABCFile abc, ASMethod connectMethod)
        {
            ASCode connectCode = connectMethod.Body.Code;
            using (var inCode = new FlashReader(connectCode.ToArray()))
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
                            pushIntIndex = abc.Constants.PushInteger(65290);
                            break;
                        }
                    }
                    outCode.Write7BitEncodedInt(pushIntIndex);
                }
                connectCode.Clear();
                connectCode.AddRange(outCode.ToArray());
            }
            RemoveDeadFalseConditions(connectCode);
        }

        public bool DisableClientEncryption()
        {
            ASInstance rc4 = _abcFiles[2].FindInstanceByName("ArcFour");
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
            ASMethod clientUnloader = _abcFiles[0].Classes[0].FindMethod("*", "Boolean")?.Method;
            if (clientUnloader == null) return false;

            ASInstance habbo = _abcFiles[1].FindInstanceByName("Habbo");
            if (habbo == null) return false;

            ASMethod domainChecker = habbo.FindMethod("*", "Boolean")?.Method;
            if (domainChecker == null) return false;

            if (domainChecker.Parameters.Count != 2) return false;
            if (domainChecker.Parameters[0].Type.ObjName != "String") return false;

            InsertEarlyReturnTrue(domainChecker);
            InsertEarlyReturnTrue(clientUnloader);
            return true;
        }
        public bool DisableExpirationDateCheck()
        {
            ABCFile abc = _abcFiles[2];
            ASInstance windowContext = abc.FindInstanceByName("WindowContext");
            if (windowContext == null) return false;

            ASCode methodCode = windowContext.Constructor.Body.Code;
            using (var inCode = new FlashReader(methodCode.ToArray()))
            using (var outCode = new FlashWriter(methodCode.Count))
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

                    methodCode.Clear();
                    methodCode.AddRange(outCode.ToArray());
                    return true;
                }
            }
            return false;
        }
        public bool ReplaceRSA(int exponent, string modulus)
        {
            ABCFile abc = _abcFiles[_abcFiles.Count - 1];
            int modulusIndex = abc.Constants.Strings.IndexOf(modulus);
            if (modulusIndex == -1)
            {
                abc.Constants.Strings.Add(modulus);
                modulusIndex = (abc.Constants.Strings.Count - 1);
            }

            string e = exponent.ToString("x");
            int exponentIndex = abc.Constants.Strings.IndexOf(e);
            if (exponentIndex == -1)
            {
                abc.Constants.Strings.Add(e);
                exponentIndex = (abc.Constants.Strings.Count - 1);
            }

            int rsaStart = 0;
            ASInstance commClass = abc.FindInstanceByName("HabboCommunicationDemo");
            ASMethod verifier = FindVerifyMethod(commClass, abc, out rsaStart);

            ASCode verifierCode = verifier.Body.Code;
            using (var inCode = new FlashReader(verifierCode.ToArray()))
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

                verifierCode.Clear();
                verifierCode.AddRange(outCode.ToArray());
                if (!searchingKeys) return true;
            }
            return false;
        }

        protected void RemoveDeadFalseConditions(ASCode code)
        {
            using (var inCode = new FlashReader(code.ToArray()))
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
                code.Clear();
                code.AddRange(outCode.ToArray());
            }
        }
        protected void InsertEarlyReturnTrue(ASMethod method)
        {
            ASCode code = method.Body.Code;
            if (code[0] == (byte)OPCode.PushTrue &&
                code[1] == (byte)OPCode.ReturnValue)
                return;

            method.Body.Code.InsertInstruction(0, OPCode.PushTrue);
            method.Body.Code.InsertInstruction(1, OPCode.ReturnValue);
        }
        protected void InsertEarlyReturnLocal(ASMethod method, int local)
        {
            OPCode getLocal = (local + OPCode.GetLocal_0);

            ASCode code = method.Body.Code;
            if (code[0] == (byte)getLocal &&
                code[1] == (byte)OPCode.ReturnValue)
                return;

            method.Body.Code.InsertInstruction(0, getLocal);
            method.Body.Code.InsertInstruction(1, OPCode.ReturnValue);
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

                ASCode methodCode = method.Body.Code;
                using (var code = new FlashReader(methodCode.ToArray()))
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