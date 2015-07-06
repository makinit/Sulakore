/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Collections.Generic;

namespace Sulakore.Inspector.Tags
{
    public class SymbolClassTag : IFlashTag
    {
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        private readonly List<Symbol> _symbols;
        public IList<Symbol> Symbols
        {
            get { return _symbols; }
        }

        public SymbolClassTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;

            _symbols = new List<Symbol>(_flash.ReadUI16());
            for (int i = 0; i < _symbols.Capacity; i++)
            {
                _symbols.Add(new Symbol(_flash.ReadSI16(),
                    _flash.ReadString()));
            }
        }

        public byte[] ToBytes()
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(BitConverter.GetBytes((ushort)_symbols.Count), 0, 2);

                byte[] symbolData = null;
                foreach (var symbol in _symbols)
                {
                    symbolData = symbol.ToBytes();
                    memoryStream.Write(symbolData, 0, symbolData.Length);
                }

                return ShockwaveFlash.ConstructTag(TagType, memoryStream.ToArray());
            }
        }
    }
}