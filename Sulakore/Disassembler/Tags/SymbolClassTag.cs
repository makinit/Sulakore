using System.Collections.Generic;

using Sulakore.Disassembler.IO;
using Sulakore.Disassembler.Records;

namespace Sulakore.Disassembler.Tags
{
    public class SymbolClassTag : FlashTag
    {
        public Dictionary<ushort, string> Symbols { get; }

        public SymbolClassTag() :
            base(FlashTagType.SymbolClass)
        {
            Symbols = new Dictionary<ushort, string>();
        }
        public SymbolClassTag(IDictionary<ushort, string> symbols) :
            this()
        {
            foreach (KeyValuePair<ushort, string> symbol in symbols)
                Symbols.Add(symbol.Key, symbol.Value);
        }

        public SymbolClassTag(FlashReader reader, TagRecord header) :
            base(reader, header)
        {
            int symbolCount = reader.ReadUInt16();
            Symbols = new Dictionary<ushort, string>(symbolCount);

            for (int i = 0; i < symbolCount; i++)
            {
                ushort characterId = reader.ReadUInt16();
                string symbolName = reader.ReadNullTerminatedString();

                if (Symbols.ContainsKey(characterId))
                {
                    symbolName =
                        $"{Symbols[characterId]},{symbolName}";
                }
                Symbols[characterId] = symbolName;
            }
        }

        protected override byte[] OnConstruct()
        {
            using (var tag = new FlashWriter())
            {
                tag.Position = 2;
                ushort symbolCount = 0;
                foreach (KeyValuePair<ushort, string> symbol in Symbols)
                {
                    string[] symbolNames = symbol.Value.Split(',');
                    foreach (string symbolName in symbolNames)
                    {
                        symbolCount++;
                        tag.Write(symbol.Key);
                        tag.WriteNullTerminatedString(symbol.Value);
                    }
                }
                tag.Position = 0;
                tag.Write(symbolCount);

                return tag.ToArray();
            }
        }
    }
}