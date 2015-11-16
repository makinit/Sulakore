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
        /// Gets the original exponent.
        /// </summary>
        public int Exponent { get; private set; }
        /// <summary>
        /// Gets the original modulus.
        /// </summary>
        public string Modulus { get; private set; }

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
        public bool ReplaceRSAKeys(int exponent, string modulus)
        {
            ABCFile abc = _abcFiles[2];
            string e = exponent.ToString();

            int modulusIndex = abc.Constants.Strings.IndexOf(modulus);
            if (modulusIndex == -1)
            {
                abc.Constants.Strings.Add(modulus);
                modulusIndex = (abc.Constants.Strings.Count - 1);
            }

            int exponentIndex = abc.Constants.Strings.IndexOf(e);
            if (exponentIndex == -1)
            {
                abc.Constants.Strings.Add(e);
                exponentIndex = (abc.Constants.Strings.Count - 1);
            }

            ASInstance commClass = abc.FindInstanceByName("HabboCommunicationDemo");
            foreach (ASTrait trait in commClass.Traits)
            {
                if (trait.TraitType != TraitType.Method) continue;
                var commMethod = ((MethodGetterSetterTrait)trait.Data).Method;

                if (commMethod.ReturnType.ObjName != "void") continue;
                if (commMethod.Parameters.Count != 1) continue;

                ASCode methodCode = commMethod.Body.Code;
                int getlexStart = methodCode.IndexOf((byte)OPCode.GetLex);

                if (getlexStart == -1) continue;
                using (var codeReader = new FlashReader(methodCode.ToArray()))
                using (var codeWriter = new FlashWriter(codeReader.Length))
                {
                    bool searchingKeys = true;
                    while (codeReader.Position != codeReader.Length)
                    {
                        OPCode op = codeReader.ReadOP();
                        codeWriter.Write(op);

                        if (op != OPCode.GetLex || !searchingKeys) continue;
                        getlexStart = (codeReader.Position - 1);

                        int getlexTypeIndex = codeReader.Read7BitEncodedInt();
                        codeWriter.Write7BitEncodedInt(getlexTypeIndex);

                        int getlexSize = (codeReader.Position - getlexStart);
                        ASMultiname getlexType = abc.Constants.Multinames[getlexTypeIndex];
                        if (getlexType?.ObjName != "KeyObfuscator") continue;

                        op = codeReader.ReadOP();
                        codeWriter.Write(op);

                        if (op != OPCode.CallProperty) continue;

                        int propIndex = codeReader.Read7BitEncodedInt();
                        int propArgCount = codeReader.Read7BitEncodedInt();
                        codeWriter.Position -= (getlexSize + 1);

                        ASMultiname propType = abc.Constants.Multinames[propIndex];
                        int indexToPush = (modulusIndex > 0 ? modulusIndex : exponentIndex);

                        codeWriter.Write((byte)OPCode.PushString);
                        codeWriter.Write7BitEncodedInt(indexToPush);

                        if (modulusIndex > 0) modulusIndex = -1;
                        else searchingKeys = false;
                    }

                    methodCode.Clear();
                    methodCode.AddRange(codeWriter.ToArray());
                    if (!searchingKeys) return true;
                }
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
                    var op = (OPCode)mapReader.ReadByte();
                    if (op != OPCode.GetLex) continue;

                    int mapTypeIndex = mapReader.Read7BitEncodedInt();
                    bool isOutgoing = (mapTypeIndex == outgoingMap.NameIndex);
                    bool isIncoming = (mapTypeIndex == incomingMap.NameIndex);
                    if (!isOutgoing && !isIncoming) continue;

                    op = (OPCode)mapReader.ReadByte();
                    if (op != OPCode.PushShort && op != OPCode.PushByte) continue;

                    var header = (ushort)mapReader.Read7BitEncodedInt();

                    op = (OPCode)mapReader.ReadByte();
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
            if (code[0] == 0x26 && code[1] == 0x48) return;

            method.Body.Code.InsertInstruction(0, OPCode.PushTrue);
            method.Body.Code.InsertInstruction(1, OPCode.ReturnValue);
        }
        protected void InsertEarlyReturnLocal(ASMethod method, int local)
        {
            OPCode getLocal = (local + OPCode.GetLocal_0);

            ASCode code = method.Body.Code;
            if (code[0] == (byte)getLocal && code[1] == 0x48) return;

            method.Body.Code.InsertInstruction(0, getLocal);
            method.Body.Code.InsertInstruction(1, OPCode.ReturnValue);
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