using System.IO;
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

        public override string Location { get; }

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

        public HFlash(byte[] data) :
            base(data)
        {
            _abcFiles = new List<ABCFile>();
            _binTags = new List<DefineBinaryDataTag>();

            OutgoingTypes = new Dictionary<ushort, ASInstance>();
            IncomingTypes = new Dictionary<ushort, ASInstance>();
        }
        public HFlash(string path) :
            this(File.ReadAllBytes(path))
        {
            Location = Path.GetFullPath(path);
        }

        public bool DisableClientEncryption()
        {
            ASInstance rc4 = _abcFiles[2].FindInstanceByName("RC4");
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
            ASMethod clientUnloader = _abcFiles[0].Classes[0].FindMethod("*", "Boolean");
            if (clientUnloader == null) return false;

            ASClass habbo = _abcFiles[1].FindClassByName("Habbo");
            if (habbo == null) return false;

            ASMethod domainChecker = habbo.FindMethod("isValidHabboDomain", "Boolean");
            if (domainChecker == null) return false;

            if (domainChecker.Parameters.Count != 1) return false;
            if (domainChecker.Parameters[0].Type.ObjName != "String") return false;

            InsertEarlyReturnTrue(domainChecker);
            InsertEarlyReturnTrue(clientUnloader);
            return true;
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
                            outCode.Write(OPCode.PushString);

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

        protected bool TryExtractMessages(ABCFile abc)
        {
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

                    var header = (ushort)mapReader.Read7BitEncodedInt();

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
            IList<ASTrait> methodTraits =
                instance.FindTraits(TraitType.Method);

            rsaStart = -1;
            foreach (ASTrait trait in methodTraits)
            {
                ASMethod method =
                    ((MethodGetterSetterTrait)trait.Data).Method;

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

                        if (type.ObjName == "RSAKey")
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
                {
                    var abcTag = (DoABCTag)tag;
                    ABCFile abc = abcTag.ABC;

                    if (abcTag.Name == "frame2")
                        TryExtractMessages(abc);

                    _abcFiles.Add(abc);
                    break;
                }

                case FlashTagType.DefineBinaryData:
                _binTags.Add((DefineBinaryDataTag)tag);
                break;
            }
            return tag;
        }
    }
}