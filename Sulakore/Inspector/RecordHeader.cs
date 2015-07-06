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

namespace Sulakore.Inspector
{
    public class RecordHeader
    {
        public int Length { get; private set; }
        public bool IsLong { get; private set; }
        public ushort Header { get; private set; }
        public TagType TagType { get; private set; }

        public int End { get; private set; }
        public int Start { get; private set; }
        
        public RecordHeader(ShockwaveFlash flash)
        {
            Start = flash.Position;
            Header = flash.ReadUI16();
            TagType = (TagType)(Header >> 6);
            Length = Header & 0x3f;

            if (Length >= 63)
                Length = flash.ReadSI32();

            End = flash.Position;
            IsLong = (Length >= 0x3f);
        }
    }
}