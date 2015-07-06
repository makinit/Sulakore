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

namespace Sulakore.Inspector.Shapes
{
    public class StyleChangeRecord : IShapeRecord
    {
        private readonly ShockwaveFlash _flash;

        public uint TypeFlag { get; private set; }
        public bool StateNewStyles { get; private set; }
        public bool StateLineStyle { get; private set; }
        public bool StateFillStyle1 { get; private set; }
        public bool StateFillStyle0 { get; private set; }
        public bool StateMoveTo { get; private set; }
        public uint MoveBits { get; private set; }
        public int MoveDeltaX { get; private set; }
        public int MoveDeltaY { get; private set; }
        public uint FillStyle0 { get; private set; }
        public uint FillStyle1 { get; private set; }
        public uint LineStyle { get; private set; }
        //
        //
        public uint FillBits { get; private set; }
        public uint LineBits { get; private set; }

        public StyleChangeRecord(ShockwaveFlash flash, int fillBits, int lineBits)
        {
            _flash = flash;

            TypeFlag = _flash.ReadUB(1);
            StateNewStyles = _flash.ReadBool();
            StateLineStyle = _flash.ReadBool();
            StateFillStyle1 = _flash.ReadBool();
            StateFillStyle0 = _flash.ReadBool();
            StateMoveTo = _flash.ReadBool();

            if (StateMoveTo)
            {
                MoveBits = _flash.ReadUB(5);
                MoveDeltaX = _flash.ReadSB((int)MoveBits);
                MoveDeltaY = _flash.ReadSB((int)MoveBits);
            }

            if (StateFillStyle0)
                FillStyle0 = _flash.ReadUB(fillBits);

            if (StateFillStyle1)
                FillStyle1 = _flash.ReadUB(fillBits);

            if (StateLineStyle)
                LineStyle = _flash.ReadUB(lineBits);

            if (StateNewStyles)
            {

                // FillStyles
                // LineStyles

                FillBits = _flash.ReadUB(4);
                LineBits = _flash.ReadUB(4);
            }
        }
    }
}