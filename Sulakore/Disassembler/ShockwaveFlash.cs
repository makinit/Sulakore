using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;


using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Tags;
using Sulakore.Disassembler.Records;
using Sulakore.Compression.SevenZip;
using Sulakore.Compression.Ionic.Zlib;
using Sulakore.Disassembler.Dictionary;
using Sulakore.Disassembler.ActionScript;
using Sulakore.Disassembler.ActionScript.Traits;
using Sulakore.Disassembler.ActionScript.Constants;

namespace Sulakore.Disassembler
{
    /// <summary>
    /// Specifies the compression standard used on a <see cref="ShockwaveFlash"/> instance.
    /// </summary>
    public enum CompressionStandard
    {
        /// <summary>
        /// Represents an uncompressed Shockwave Flash(SWF) file.
        /// </summary>
        None = 0x46,
        /// <summary>
        /// Represents a Shockwave Flash(SWF) file compressed by using the ZLIB open standard. (SWF 6+)
        /// </summary>
        ZLIB = 0x43,
        /// <summary>
        /// Represents a Shockwave Flash(SWF) file compressed by using the LZMA open standard. (SWF 13+)
        /// </summary>
        LZMA = 0x5A
    }

    /// <summary>
    /// Represents a Shockwave Flash object containing modifiable tag data.
    /// </summary>
    public class ShockwaveFlash : IDisposable
    {
        private int _frameEndPos;
        private byte[] _flashData, _frameData;

        /// <summary>
        /// Gets the <see cref="FlashReader"/> instance containing the Shockwave Flash(SWF) binary data to be read.
        /// </summary>
        public FlashReader Reader { get; }
        /// <summary>
        /// Gets the <see cref="List{T}"/> of tags that make up the content of the Shockwave Flash(SWF) file.
        /// </summary>
        public List<FlashTag> Tags { get; }
        /// <summary>
        /// Gets the global character dictionary that associates character ids to objects.
        /// </summary>
        public FlashDictionary Dictionary { get; }

        /// <summary>
        /// Gets or sets the location for the Shockwave Flash(SWF) file.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="CompressionStandard"/> value that will be used to determine the compresison method used when <see cref="Compress"/> is invoked.
        /// </summary>
        public CompressionStandard CompressWith { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// Gets or sets the frame rate.
        /// </summary>
        public ushort FrameRate { get; set; }
        /// <summary>
        /// Gets or sets the frame count.
        /// </summary>
        public ushort FrameCount { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is compressed.
        /// </summary>
        public bool IsCompressed { get; private set; }
        /// <summary>
        /// Gets the total length/size of the uncompressed Shockwave Flash(SWF) file including the header(8 bytes).
        /// </summary>
        public uint FileLength { get; private set; }
        /// <summary>
        /// Gets the signature that contains the type of compression used, if any.
        /// </summary>
        public string Signature { get; private set; }
        /// <summary>
        /// Gets the <see cref="RectangleRecord"/> that contains the frame's size in twips.
        /// </summary>
        public RectangleRecord FrameSize { get; private set; }

        private readonly List<ABCFile> _abcFiles;
        public IReadOnlyList<ABCFile> ABCFiles => _abcFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShockwaveFlash"/> class based on the specified Shockwave Flash(SWF) file path.
        /// </summary>
        /// <param name="path">The file path of the Shockwave Flash(SWF) file.</param>
        public ShockwaveFlash(string path) :
            this(File.ReadAllBytes(path))
        {
            Location = Path.GetFullPath(path);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ShockwaveFlash"/> class based on the specified array of bytes.
        /// </summary>
        /// <param name="data">The data containing the raw Shockwave Flash(SWF) file.</param>
        public ShockwaveFlash(byte[] data)
        {
            _flashData = data;
            _abcFiles = new List<ABCFile>();

            Reader = new FlashReader(data);
            Tags = new List<FlashTag>();
            Dictionary = new FlashDictionary();

            Signature = Reader.ReadString(3);
            CompressWith = (CompressionStandard)Signature[0];

            Version = Reader.ReadByte();
            FileLength = Reader.ReadUInt32();

            if (CompressWith ==
                CompressionStandard.None)
            {
                ReadFrameInformation();
            }
            else IsCompressed = true;
        }

        /// <summary>
        /// Compresses the Shockwave Flash(SWF) file using <see cref="CompressWith"/> to decide the compression method.
        /// </summary>
        public byte[] Compress()
        {
            return StitchFlashDataWithHeader(CompressWith, true);
        }
        /// <summary>
        /// Decompresses the Shockwave Flash(SWF) file if necessary.
        /// </summary>
        public void Decompress()
        {
            if (!IsCompressed) return;

            _flashData =
                StitchFlashDataWithHeader(CompressWith, false);

            Reader.ResetBuffer(_flashData);
            ReadFrameInformation();
            IsCompressed = false;
        }
        /// <summary>
        /// Reconstructs the Shockwave Flash(SWF) file using the tags from <see cref="Tags"/>.
        /// </summary>
        public byte[] Reconstruct()
        {
            using (var flash = new FlashWriter())
            {
                flash.WriteUTF8SimpleString("FWS");
                flash.Write(Version);

                flash.Position += 4;
                flash.Write(_frameData);

                foreach (FlashTag tag in Tags)
                    flash.Write(tag.ToArray());

                flash.Position = 4;
                flash.Write(flash.Length);

                _flashData = flash.ToArray();
                Reader.ResetBuffer(_flashData);
                return _flashData;
            }
        }

        /// <summary>
        /// Reads the flash tags that make up the content of the Shockwave Flash(SWF) file.
        /// </summary>
        public virtual List<FlashTag> ReadTags()
        {
            Tags.Clear();
            _abcFiles.Clear();
            Reader.Position = _frameEndPos;
            while (Reader.Position != Reader.Length)
            {
                var header = new TagRecord(Reader);
                int expectedPosition = (header.Body.Length + Reader.Position);

                Tags.Add(ReadTag(Reader, header));
                if (Reader.Position != expectedPosition)
                {
                    int lastTagIndex = (Tags.Count - 1);
                    throw new Exception($"Incorrect position has been reached at {Reader.Position}.\r\n" +
                        $"Expected Position: {expectedPosition} | TagType: {Tags[lastTagIndex].Header.TagType} | Tag Index: {lastTagIndex}");
                }
            }
            return Tags;
        }
        protected virtual FlashTag ReadTag(FlashReader reader, TagRecord header)
        {
            FlashTag tag = null;
            switch (header.TagType)
            {
                default:
                tag = new UnknownTag(Reader, header);
                break;

                case FlashTagType.DoABC:
                tag = new DoABCTag(Reader, header);
                _abcFiles.Add(((DoABCTag)tag).ABC);
                break;

                case FlashTagType.DefineBitsLossless2:
                tag = new DefineBitsLossless2Tag(Reader, header);
                break;

                case FlashTagType.DefineBinaryData:
                tag = new DefineBinaryDataTag(Reader, header);
                break;
            }

            var character = (tag as ICharacter);
            if (character != null)
            {
                // Add ICharacter tag to the global dictionary.
                Dictionary.Characters[
                    character.CharacterId] = character;
            }
            return tag;
        }

        public string GetHash(ASClass asClass)
        {
            using (var hashStream = new MemoryStream())
            using (var hashInput = new BinaryWriter(hashStream))
            {
                WriteClassHashData(hashInput, asClass);
                return GetHash(hashStream.ToArray());
            }
        }
        protected string GetHash(byte[] hashData)
        {
            using (var md5 = MD5.Create())
            {
                hashData = md5.ComputeHash(hashData);
                string hex = BitConverter.ToString(hashData);
                return hex.Replace("-", string.Empty).ToLower();
            }
        }

        protected virtual void WriteClassHashData(BinaryWriter hashInput, ASClass asClass)
        {
            ABCFile abc = asClass.ABC;
            WriteTraitsHashData(hashInput, asClass);
            WriteMethodHashData(hashInput, asClass.Constructor, true);

            WriteTraitsHashData(hashInput, asClass.Instance);
            WriteMethodHashData(hashInput, asClass.Instance.Constructor, true);

            string superClassObjName = asClass.Instance.SuperType?.ObjName;
            if (!string.IsNullOrWhiteSpace(superClassObjName) && superClassObjName != "Object")
            {
                ASClass superClass = abc
                    .FindClassByName(superClassObjName);

                if (superClass != null)
                {
                    WriteMultinameHashData(hashInput, asClass.Instance.SuperType);
                    WriteClassHashData(hashInput, superClass);
                }
            }

            hashInput.Write((byte)asClass.Instance.ClassInfo);
            hashInput.Write((byte)asClass.Instance.Type.MultinameType);
            hashInput.Write((byte)asClass.Instance.ProtectedNamespace.NamespaceType);
        }
        protected virtual void WriteValueSlotHashData(BinaryWriter hashInput, IValueSlot valueSlot)
        {
            hashInput.Write((byte)valueSlot.ValueType);
            switch (valueSlot.ValueType)
            {
                case ConstantType.True:
                case ConstantType.False:
                hashInput.Write((bool)valueSlot.Value);
                break;

                case ConstantType.String:
                hashInput.Write((string)valueSlot.Value);
                break;

                case ConstantType.Double:
                hashInput.Write((double)valueSlot.Value);
                break;

                case ConstantType.Integer:
                hashInput.Write((int)valueSlot.Value);
                break;

                case ConstantType.UInteger:
                hashInput.Write((uint)valueSlot.Value);
                break;
            }
        }
        protected virtual void WriteMultinameHashData(BinaryWriter hashInput, ASMultiname asMultiname)
        {
            hashInput.Write((byte)asMultiname.MultinameType);
            switch (asMultiname.ObjName)
            {
                case "int":
                case "Class":
                case "Array":
                case "String":
                case "Boolean":
                case "Function":
                {
                    hashInput.Write(asMultiname.ObjName);
                    break;
                }
            }
        }
        protected virtual void WriteTraitsHashData(BinaryWriter hashInput, TraitContainer traitContainer)
        {
            hashInput.Write(traitContainer.Traits.Count);
            foreach (ASTrait trait in traitContainer.Traits)
            {
                hashInput.Write((byte)trait.TraitType);
                hashInput.Write((byte)trait.Attributes);
                hashInput.Write(trait.Id);
                switch (trait.TraitType)
                {
                    case TraitType.Class:
                    {
                        var classTrait = (ClassTrait)trait.Data;
                        WriteClassHashData(hashInput, classTrait.Class);
                        break;
                    }
                    case TraitType.Method:
                    case TraitType.Getter:
                    case TraitType.Setter:
                    {
                        var mgsTrait = (MethodGetterSetterTrait)trait.Data;
                        WriteMethodHashData(hashInput, mgsTrait.Method, true);
                        break;
                    }
                    case TraitType.Slot:
                    case TraitType.Constant:
                    {
                        var slotConstTrait = (SlotConstantTrait)trait.Data;
                        WriteValueSlotHashData(hashInput, slotConstTrait);
                        break;
                    }
                    case TraitType.Function:
                    {
                        var funcTrait = (FunctionTrait)trait.Data;
                        WriteMethodHashData(hashInput, funcTrait.Function, true);
                        break;
                    }
                }
            }
        }
        protected virtual void WriteMethodHashData(BinaryWriter hashInput, ASMethod asMethod, bool writeInstructions)
        {
            WriteTraitsHashData(hashInput, asMethod.Body);
            hashInput.Write(asMethod.Body.Exceptions.Count);
            hashInput.Write(asMethod.Body.MaxStack);
            hashInput.Write(asMethod.Body.LocalCount);
            hashInput.Write(asMethod.Body.MaxScopeDepth);
            hashInput.Write(asMethod.Body.InitialScopeDepth);

            hashInput.Write(asMethod.Parameters.Count);
            foreach (ASParameter parameter in asMethod.Parameters)
            {
                if (parameter.IsOptional)
                {
                    hashInput.Write(parameter.IsOptional);
                    WriteValueSlotHashData(hashInput, parameter);
                }

                if (parameter.Type != null)
                    WriteMultinameHashData(hashInput, parameter.Type);
            }

            if (writeInstructions)
            {
                using (var codeOutput =
                    new FlashReader(asMethod.Body.Bytecode))
                {
                    while (codeOutput.IsDataAvailable)
                    {
                        OPCode op = codeOutput.ReadOP();
                        object[] values = codeOutput.ReadValues(op);
                        hashInput.Write((byte)op);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a byte array representing the Shockwave Flash(SWF) file.
        /// </summary>
        public byte[] ToByteArray() => _flashData;
        /// <summary>
        /// Reads the frame information like size, rate, and count.
        /// </summary>
        protected void ReadFrameInformation()
        {
            Reader.Position = 8;

            FrameSize = new RectangleRecord(Reader);
            FrameRate = (ushort)(Reader.ReadUInt16() >> 8);
            FrameCount = Reader.ReadUInt16();

            _frameEndPos = Reader.Position;
            Reader.Position = 8;

            _frameData = Reader.ReadBytes(_frameEndPos - 8);
        }
        /// <summary>
        /// Stitches the Shockwave Flash(SWF) file header containing basic information, with the compressed/uncompressed content of the file. 
        /// </summary>
        /// <param name="standard">The compression/decompression standard to use.</param>
        /// <param name="isCompressing">if set to <c>true</c> [is compressing].</param>
        /// <returns></returns>
        protected byte[] StitchFlashDataWithHeader(CompressionStandard standard, bool isCompressing)
        {
            if (isCompressing &&
                standard == CompressionStandard.None)
            {
                CompressWith = CompressionStandard.ZLIB;
                standard = CompressWith;
            }

            var flashHeader = new byte[8];
            Buffer.BlockCopy(_flashData, 0, flashHeader, 0, 8);
            flashHeader[0] = (isCompressing ? (byte)standard : (byte)'F');

            var flashBody = new byte[_flashData.Length - 8];
            Buffer.BlockCopy(_flashData, 8, flashBody, 0, flashBody.Length);

            byte[] body = null;
            switch (standard)
            {
                default:
                {
                    throw new InvalidOperationException(
                        "Invalid compression/decompression standard was specified: " + standard);
                }
                case CompressionStandard.ZLIB:
                {
                    if (isCompressing) body = ZlibStream.CompressBuffer(flashBody);
                    if (!isCompressing) body = ZlibStream.UncompressBuffer(flashBody);
                    break;
                }
                case CompressionStandard.LZMA:
                {
                    if (isCompressing) body = LZMA.CompressBuffer(flashBody);
                    if (!isCompressing) body = LZMA.DecompressBuffer(flashBody, (int)FileLength - 8);
                    break;
                }
            }

            var buffer = new byte[8 + body.Length];
            Buffer.BlockCopy(flashHeader, 0, buffer, 0, 8);
            Buffer.BlockCopy(body, 0, buffer, 8, body.Length);
            return buffer;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            try
            {
                if (disposing)
                {
                    ((IDisposable)Reader).Dispose();
                }
            }
            finally { IsDisposed = true; }
        }
    }
}