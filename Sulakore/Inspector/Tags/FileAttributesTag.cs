﻿/* Copyright

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

namespace Sulakore.Inspector.Tags
{
    public class FileAttributesTag : IFlashTag
    {
        private readonly byte[] _data;
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        public bool UseDirectBlit { get; private set; }
        public bool UseGPU { get; private set; }
        public bool HasMetadata { get; private set; }
        public bool IsAS3 { get; private set; }
        public bool UseNetwork { get; private set; }

        public FileAttributesTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;

            _data = _flash.ReadUI8Block(header.Length, _flash.Position);

            /* Reserved */
            _flash.ReadUB(1);
            UseDirectBlit = (_flash.ReadUB(1) == 1);
            UseGPU = (_flash.ReadUB(1) == 1);
            HasMetadata = (_flash.ReadUB(1) == 1);
            IsAS3 = (_flash.ReadUB(1) == 1);
            /* Reserved */
            _flash.ReadUB(2);
            UseNetwork = (_flash.ReadUB(1) == 1);
            /* Reserved */
            _flash.ReadUB(24);
        }

        public byte[] ToBytes()
        {
            return ShockwaveFlash.ConstructTag(TagType,
                _flash.ReadUI8Block(_header.Length, Position));
        }
    }
}