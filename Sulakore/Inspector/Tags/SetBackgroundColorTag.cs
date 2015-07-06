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

using System.Drawing;

namespace Sulakore.Inspector.Tags
{
    public class SetBackgroundColorTag : IFlashTag
    {
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        public Color Background { get; set; }

        public SetBackgroundColorTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;

            Background = Color.FromArgb(_flash.ReadUI8(),
                _flash.ReadUI8(), _flash.ReadUI8());
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[3];
            buffer[0] = Background.R;
            buffer[1] = Background.G;
            buffer[2] = Background.B;

            return ShockwaveFlash.ConstructTag(TagType, buffer);
        }
    }
}