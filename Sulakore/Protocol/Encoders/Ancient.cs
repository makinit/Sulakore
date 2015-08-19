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
    public static class Ancient
    {
        public static byte[] CypherShort(ushort value)
        {
            return new byte[2] { (byte)(64 + (value >> 6 & 63)), (byte)(64 + (value >> 0 & 63)) };
        }
        public static byte[] CypherShort(byte[] data, int offset, ushort value)
        {
            offset = offset > data.Length ? data.Length : offset < 0 ? 0 : offset;

            var buffer = new byte[data.Length + 2];
            for (int i = 0, j = 0; j < buffer.Length; j++)
            {
                if (j != offset) buffer[j] = data[i++];
                else
                {
                    byte[] ToInsert = CypherShort(value);
                    buffer[j++] = ToInsert[0];
                    buffer[j] = ToInsert[1];
                }
            }
            return buffer;
        }
        public static ushort DecypherShort(byte[] Data)
        {
            return DecypherShort(Data, 0);
        }
        public static ushort DecypherShort(string Encoded)
        {
            return DecypherShort(new byte[2] { (byte)Encoded[0], (byte)Encoded[1] }, 0);
        }
        public static ushort DecypherShort(byte[] Data, int Offset)
        {
            return (ushort)(Data.Length > 1 ? (Data[Offset + 1] - 64 + (Data[Offset] - 64) * 64) : 0);
        }
        public static ushort DecypherShort(byte First, byte Second)
        {
            return DecypherShort(new byte[2] { First, Second }, 0);
        }

        public static byte[] CypherInt(int Value)
        {
            int Length = 1;
            int NonNegative = Value < 0 ? -(Value) : Value;
            byte[] Buffer = new byte[6] { (byte)(64 + (NonNegative & 3)), 0, 0, 0, 0, 0 };

            for (NonNegative >>= 2; NonNegative != 0; NonNegative >>= 6, Length++)
                Buffer[Length] = (byte)(64 + (NonNegative & 63));

            Buffer[0] = (byte)(Buffer[0] | Length << 3 | (Value >= 0 ? 0 : 4));

            byte[] ZerosTrimmed = new byte[Length];
            for (int i = 0; i < Length; i++)
                ZerosTrimmed[i] = Buffer[i];

            return ZerosTrimmed;
        }
        public static byte[] CypherInt(byte[] Base, int Offset, int Value)
        {
            Offset = Offset > Base.Length ? Base.Length : Offset < 0 ? 0 : Offset;

            byte[] ToInsert = CypherInt(Value);
            byte[] Data = new byte[Base.Length + ToInsert.Length];
            for (int i = 0, j = 0; j < Data.Length; j++)
            {
                if (j != Offset) Data[j] = Base[i++];
                else
                {
                    for (int k = 0, l = j; k < ToInsert.Length; k++, l++)
                        Data[l] = ToInsert[k];
                    j += ToInsert.Length - 1;
                }
            }
            return Data;
        }
        public static int DecypherInt(byte[] Data)
        {
            return DecypherInt(Data, 0);
        }
        public static int DecypherInt(string Encoded)
        {
            byte[] Data = new byte[Encoded.Length];
            for (int i = 0; i < Encoded.Length; i++)
                Data[i] = (byte)Encoded[i];
            return DecypherInt(Data, 0);
        }
        public static int DecypherInt(byte[] Data, int Offset)
        {
            int Length = (Data[Offset] >> 3) & 7;
            int Decoded = Data[Offset] & 3;
            bool IsNegative = (Data[Offset] & 4) == 4;
            for (int i = 1, j = Offset + 1, k = 2; i < Length; i++, j++)
            {
                if (Length > Data.Length - Offset) break;
                Decoded |= (Data[j] & 63) << k;
                k = 2 + (6 * i);
            }
            return IsNegative ? -(Decoded) : Decoded;
        }
        public static int DecypherInt(byte First, params byte[] Data)
        {
            byte[] Buffer = new byte[Data.Length + 1];
            Buffer[0] = First;
            for (int i = 0; i < Buffer.Length; i++)
                Buffer[i + 1] = Data[i];

            return DecypherInt(Buffer, 0);
        }
    }
}