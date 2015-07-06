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

namespace Sulakore.Inspector.Tags
{
    public class DefineFont3Tag : IFlashTag
    {
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        public ushort FontId { get; set; }
        public bool FontFlagsHasLayout{ get; private set; }
        public bool FontFlagsShiftJIS{ get; private set; }
        public bool FontFlagsSmallText{ get; private set; }
        public bool FontFlagsANSI{ get; private set; }
        public bool FontFlagsWideOffsets{ get; private set; }
        public bool FontFlagsWideCodes{ get; private set; }
        public bool FontFlagsItalic{ get; private set; }
        public bool FontFlagsBold{ get; private set; }
        public LanguageCode Language { get; set; }

        public byte FontNameLen{ get; private set; }
        public string FontName { get; set; }

        public ushort NumGlyphs { get; set; }
        public uint OffsetTable{ get; private set; }
        public uint CodeTableOffset{ get; private set; }
        //GlyphShapeTable - we must go deeeepper -> Shape.cs

        public DefineFont3Tag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}