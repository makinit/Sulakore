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

namespace Sulakore.Protocol.Encoders
{
    public static class BigEndian
    {
        public static ushort ToUI16(byte[] data)
        {
            return ToUI16(data, 0);
        }
        public static byte[] FromUI16(ushort value)
        {
            return new[] { (byte)(value >> 8), (byte)value };
        }
        public static ushort ToUI16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) + data[offset + 1]);
        }

        public static int ToSI32(byte[] data)
        {
            return ToSI32(data, 0);
        }
        public static byte[] FromSI32(int value)
        {
            return new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }
        public static int ToSI32(byte[] data, int offset)
        {
            return ((data[offset] << 24) +
                (data[offset + 1] << 16) +
                (data[offset + 2] << 8) +
                data[offset + 3]);
        }
    }
}