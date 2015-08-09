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

namespace Sulakore.Protocol
{
    public class HScripture : HPacketBase
    {
        public override byte[] Body
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override HDestination Destination
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override ushort Header
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsCorrupted
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int Length
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override int Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanRead<T>(int index)
        {
            throw new NotImplementedException();
        }

        public override bool ReadBoolean(ref int index)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadBytes(int length, ref int index)
        {
            throw new NotImplementedException();
        }

        public override int ReadInteger(ref int index)
        {
            throw new NotImplementedException();
        }

        public override ushort ReadShort(ref int index)
        {
            throw new NotImplementedException();
        }

        public override string ReadString(ref int index)
        {
            throw new NotImplementedException();
        }

        public override void Remove<T>(int index)
        {
            throw new NotImplementedException();
        }

        public override void Replace<T>(int index, object chunk)
        {
            throw new NotImplementedException();
        }
    }
}