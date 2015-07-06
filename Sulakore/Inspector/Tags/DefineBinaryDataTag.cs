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
    public class DefineBinaryDataTag : IFlashTag
    {
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;

        public byte[] Data { get; set; }
        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        public ushort CharacterId { get; set; }

        public DefineBinaryDataTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;

            CharacterId = _flash.ReadUI16();
            /* Reserved */ _flash.ReadUI32();

            Data = _flash.ReadUI8Block((int)header.Length - 6);
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Data.Length + 6];
            Buffer.BlockCopy(BitConverter.GetBytes(CharacterId), 0, buffer, 0, 2);
            Buffer.BlockCopy(Data, 0, buffer, 6, Data.Length);

            return ShockwaveFlash.ConstructTag(TagType, buffer);
        }
    }
}