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
    public class ProductInfoTag : IFlashTag
    {
        private readonly RecordHeader _header;
        private readonly ShockwaveFlash _flash;
        private static readonly DateTime _epoch;

        public int Position { get; private set; }
        public TagType TagType { get; private set; }

        public Product Product { get; set; }
        public Edition Edition { get; set; }
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public uint BuildLow { get; set; }
        public uint BuildHigh { get; set; }
        public DateTime CompilationDate { get; set; }

        static ProductInfoTag()
        {
            _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        public ProductInfoTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;
            _header = header;

            TagType = _header.TagType;
            Position = _flash.Position;

            Product = (Product)_flash.ReadUI32();
            Edition = (Edition)_flash.ReadUI32();
            MajorVersion = _flash.ReadUI8();
            MinorVersion = _flash.ReadUI8();
            BuildLow = _flash.ReadUI32();
            BuildHigh = _flash.ReadUI32();
            CompilationDate = _epoch.AddMilliseconds(_flash.ReadUI64());
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[26];
            Buffer.BlockCopy(BitConverter.GetBytes((uint)Product), 0, buffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)Edition), 0, buffer, 4, 4);
            buffer[8] = MajorVersion;
            buffer[9] = MinorVersion;
            Buffer.BlockCopy(BitConverter.GetBytes(BuildLow), 0, buffer, 10, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(BuildHigh), 0, buffer, 14, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ulong)(CompilationDate - _epoch).TotalMilliseconds), 0, buffer, 18, 8);

            return ShockwaveFlash.ConstructTag(TagType, buffer);
        }
    }
}