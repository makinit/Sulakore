using System.Collections.Generic;

namespace Sulakore.Disassembler.Dictionary
{
    public class FlashDictionary
    {
        public Dictionary<ushort, string> SymbolNames { get; }
        public Dictionary<ushort, ICharacter> Characters { get; }

        public FlashDictionary()
        {
            SymbolNames = new Dictionary<ushort, string>();
            Characters = new Dictionary<ushort, ICharacter>();
        }
    }
}