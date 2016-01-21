using System.Text;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;
using Sulakore.Disassembler.ActionScript;

namespace Sulakore.Disassembler.Tags
{
    /// <summary>
    /// Defines a series of bytecodes to be exectued by the ActionScript virtual machine.
    /// </summary>
    public class DoABCTag : FlashTag
    {
        /// <summary>
        /// Gets or sets the 32-bit flags value.
        /// </summary>
        public uint Flags { get; set; }
        /// <summary>
        /// Gets or sets the name associated to the <see cref="ABCFile"/> contained in this tag.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="ABCFile"/> contained in this tag.
        /// </summary>
        public ABCFile ABC { get; set; }

        public DoABCTag(uint flags, string name) :
            base(FlashTagType.DoABC)
        { }

        public DoABCTag(FlashReader reader, TagRecord header)
            : base(reader, header)
        {
            Flags = reader.ReadUInt32();
            Name = reader.ReadNullTerminatedString();

            int nameLength = (Encoding.UTF8.GetByteCount(Name) + 1);
            byte[] abcData = reader.ReadBytes(header.Body.Length - (nameLength + 4));

            ABC = new ABCFile(abcData);
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter())
            {
                tag.Write(Flags);
                tag.WriteNullTerminatedString(Name);
                tag.Write(ABC.ToByteArray());

                return tag.ToArray();
            }
        }
    }
}