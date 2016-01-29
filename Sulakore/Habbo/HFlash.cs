using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Sulakore.Disassembler;
using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.ActionScript;
using Sulakore.Disassembler.ActionScript.Traits;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Habbo
{
    public class HFlash : ShockwaveFlash
    {
        protected readonly Dictionary<ASInstance, ASClass> _incomingParsersCache;
        private readonly Dictionary<ASClass, List<Tuple<ASMethod, int>>> _messageReferencesCache;

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
            _incomingParsersCache = new Dictionary<ASInstance, ASClass>();
            _messageReferencesCache = new Dictionary<ASClass, List<Tuple<ASMethod, int>>>();

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
            if (OutgoingTypes.Count > 0 &&
                IncomingTypes.Count > 0) return true;

            ABCFile abc = ABCFiles[2];
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
        public bool BypassRemoteHostCheck()
        {
            ABCFile abc = ABCFiles[2];
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
            ASInstance rc4 = ABCFiles[2].FindInstanceByName("ArcFour");
            if (rc4 == null) ABCFiles[2].FindInstanceByName("RC4");
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
            ASMethod unloadingMethod = ABCFiles[0].Classes[0]
                .FindMethod("*", "Boolean")?.Method;

            if (unloadingMethod == null)
                return false;

            ASClass habboClass = ABCFiles[1].FindClassByName("Habbo");
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
            ABCFile abc = ABCFiles[2];
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
            ABCFile abc = ABCFiles[2];
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

        protected void ScanForMessageReferences(ABCFile abc)
        {
            if (!FindMessageInstances()) return;
            if (_messageReferencesCache.Count > 0) return;

            var messageClasses = new Dictionary<string, ASClass>(
                OutgoingTypes.Count + IncomingTypes.Count);

            foreach (ASClass outClass in OutgoingTypes.Values)
            {
                messageClasses.Add(
                    outClass.Instance.Type.ObjName, outClass);
            }
            foreach (ASClass inClass in IncomingTypes.Values)
            {
                messageClasses.Add(
                    inClass.Instance.Type.ObjName, inClass);
            }
            foreach (ASClass asClass in abc.Classes)
            {
                ASInstance asInstance = asClass.Instance;
                if (asInstance.ClassInfo.HasFlag(ClassFlags.Interface)) continue;
                ScanForMessageReference(messageClasses, asClass, asClass.Constructor, 0);
                ScanForMessageReference(messageClasses, asClass, asClass.Instance.Constructor, 0);

                if (asInstance.Traits.Count < 1) continue;
                for (int i = 0; i < asInstance.Traits.Count; i++)
                {
                    ASTrait trait = asInstance.Traits[i];
                    if (trait.TraitType != TraitType.Method) continue;

                    var mgsTrait = (MethodGetterSetterTrait)trait.Data;
                    ScanForMessageReference(messageClasses, asClass, mgsTrait.Method, i);
                }
            }
        }
        protected void ScanForMessageReference(Dictionary<string, ASClass> messageClasses, ASClass asClass, ASMethod method, int traitIndex, int messageRefCount = 0)
        {
            ABCFile abc = asClass.ABC;
            ASClass messageClass = null;
            using (var outCode = new FlashReader(method.Body.Bytecode))
            {
                while (outCode.IsDataAvailable)
                {
                    OPCode op = outCode.ReadOP();
                    object[] values = outCode.ReadValues(op);
                    switch (op)
                    {
                        case OPCode.NewFunction:
                        {
                            var newFuncIndex = (int)values[0];
                            ASMethod newFuncMethod = abc.Methods[newFuncIndex];
                            ScanForMessageReference(messageClasses, asClass, newFuncMethod, traitIndex, messageRefCount);
                            break;
                        }
                        case OPCode.ConstructProp:
                        {
                            var constructPropIndex = (int)values[0];
                            if (messageClass != null)
                            {
                                ASMultiname constructPropType =
                                    abc.Constants.Multinames[constructPropIndex];

                                if (constructPropType.ObjName == messageClass.Instance.Type.ObjName)
                                {
                                    if (!_messageReferencesCache.ContainsKey(messageClass))
                                        _messageReferencesCache[messageClass] = new List<Tuple<ASMethod, int>>();

                                    _messageReferencesCache[messageClass].Add(
                                        new Tuple<ASMethod, int>(method, (traitIndex + (++messageRefCount))));
                                }
                                messageClass = null;
                            }
                            break;
                        }
                        case OPCode.FindPropStrict:
                        {
                            var findPropStrictIndex = (int)values[0];
                            string findPropStrictObjName = abc.Constants
                                .Multinames[findPropStrictIndex].ObjName;

                            if (messageClasses.ContainsKey(findPropStrictObjName))
                            {
                                messageClass = messageClasses[findPropStrictObjName];

                                // Incoming messages currently not supported.
                                if (IncomingTypes.ContainsValue(messageClass))
                                    messageClass = null;
                            }
                            break;
                        }
                    }
                }
            }
        }

        protected void InsertEarlyReturnTrue(ASMethod method)
        {
            method.Body.Bytecode[0] = (byte)OPCode.PushTrue;
            method.Body.Bytecode[1] = (byte)OPCode.ReturnValue;
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

        public ASClass GetIncomingParser(ASInstance incomingInstance)
        {
            if (_incomingParsersCache.ContainsKey(incomingInstance))
                return _incomingParsersCache[incomingInstance];

            ASClass parserClass = null;
            ABCFile abc = incomingInstance.ABC;
            try
            {
                using (var codeOut = new FlashReader(
                    incomingInstance.Constructor.Body.Bytecode))
                {
                    while (codeOut.IsDataAvailable)
                    {
                        OPCode op = codeOut.ReadOP();
                        object[] values = codeOut.ReadValues(op);
                        if (op != OPCode.GetLex) continue;

                        var getLexIndex = (int)values[0];
                        ASMultiname getLexName = abc.Constants.Multinames[getLexIndex];
                        parserClass = abc.FindClassByName(getLexName.ObjName);
                        if (parserClass != null) return parserClass;
                        break;
                    }
                }

                ASInstance incomingSuperInstance = abc.FindInstanceByName(
                    incomingInstance.SuperType.ObjName);

                ASMultiname parserReturnType = incomingSuperInstance
                    .FindGetter("parser").Method.ReturnType;

                SlotConstantTrait parserSlot = incomingSuperInstance
                    .FindSlot("*", parserReturnType.ObjName);

                foreach (ASTrait trait in incomingInstance.Traits)
                {
                    if (trait.TraitType != TraitType.Method) continue;

                    var mgsTrait = (MethodGetterSetterTrait)trait.Data;
                    if (mgsTrait.Method.Parameters.Count != 0) continue;

                    using (var codeOut = new FlashReader(
                        mgsTrait.Method.Body.Bytecode))
                    {
                        while (codeOut.IsDataAvailable)
                        {
                            OPCode op = codeOut.ReadOP();
                            object[] values = codeOut.ReadValues(op);
                            if (op != OPCode.GetLex) continue;

                            var getLexIndex = (int)values[0];
                            ASMultiname getLexType = abc.Constants.Multinames[getLexIndex];
                            if (getLexType.ObjName != parserSlot.ObjName) continue;

                            parserClass = abc.FindClassByName(mgsTrait.Method.ReturnType.ObjName);
                            if (parserClass != null) return parserClass;
                            break;
                        }
                    }
                }
                return parserClass;
            }
            finally
            {
                if (parserClass != null)
                    _incomingParsersCache[incomingInstance] = parserClass;
            }
        }
        public string GetHash(ASClass asClass, bool referenceScan, bool isOutgoing)
        {
            using (var hashStream = new MemoryStream())
            using (var hashInput = new BinaryWriter(hashStream))
            {
                hashInput.Write(isOutgoing);
                WriteHashData(hashInput, asClass, isOutgoing);

                if (referenceScan)
                {
                    ScanForMessageReferences(asClass.ABC);
                    if (_messageReferencesCache.ContainsKey(asClass))
                    {
                        List<Tuple<ASMethod, int>> messageReferences = _messageReferencesCache[asClass];
                        if (asClass.Instance.Type.ObjName == "RenderRoomMessageComposer")
                        {

                        }
                        foreach (Tuple<ASMethod, int> messageReference in messageReferences)
                        {
                            ASMethod referencingMethod = messageReference.Item1;
                            int referenceId = messageReference.Item2;

                            WriteMethodHashData(hashInput, referencingMethod, false);
                            hashInput.Write(referenceId);
                        }
                    }
                }
                return GetHash(hashStream.ToArray());
            }
        }

        protected virtual void WriteHashData(BinaryWriter hashInput, ASClass asClass, bool isOutgoing)
        {
            if (isOutgoing)
            {
                string outgoingTypeName =
                    asClass.Instance.Type.ObjName;

                if (outgoingTypeName.EndsWith("Composer"))
                {
                    hashInput.Write(outgoingTypeName);
                    return;
                }
            }
            else
            {
                ASClass parserClass =
                    GetIncomingParser(asClass.Instance);

                if (parserClass != null)
                    WriteClassHashData(hashInput, parserClass);
            }
            WriteClassHashData(hashInput, asClass);
        }
    }
}