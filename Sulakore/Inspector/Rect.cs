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

namespace Sulakore.Inspector
{
    public class Rect
    {
        private readonly byte[] _data;

        public int X { get; private set; }
        public int Y { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int TwipsWidth { get; private set; }
        public int TwipsHeight { get; private set; }

        public Rect(ShockwaveFlash flash)
        {
            int pos = flash.Position;
            int nBits = (int)flash.ReadUB(5);

            X = flash.ReadSB(nBits);
            TwipsWidth = flash.ReadSB(nBits);
            Width = TwipsWidth / 20;

            Y = flash.ReadSB(nBits);
            TwipsHeight = flash.ReadSB(nBits);
            Height = TwipsHeight / 20;

            _data = flash.ReadUI8Block(flash.Position - pos, pos);
        }

        public byte[] ToBytes()
        {
            return _data;
        }
    }
}