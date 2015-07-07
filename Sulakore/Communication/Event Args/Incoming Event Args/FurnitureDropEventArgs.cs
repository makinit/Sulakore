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
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureDropEventArgs : InterceptedEventArgs
    {
        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public int OwnerId { get; private set; }
        public HPoint Tile { get; private set; }
        public bool IsRental { get; private set; }
        public string OwnerName { get; private set; }
        public HDirection Direction { get; private set; }

        public FurnitureDropEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public FurnitureDropEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public FurnitureDropEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public FurnitureDropEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = Packet.ReadInteger();
            TypeId = Packet.ReadInteger();

            int x = Packet.ReadInteger();
            int y = Packet.ReadInteger();

            Direction = (HDirection)Packet.ReadInteger();
            Tile = new HPoint(x, y, double.Parse(Packet.ReadString()));

            Packet.ReadString();
            Packet.ReadInteger();
            Packet.ReadInteger();
            Packet.ReadString();

            IsRental = (Packet.ReadInteger() != 1);
            Packet.ReadInteger();

            OwnerId = Packet.ReadInteger();
            OwnerName = Packet.ReadString();
        }
        public FurnitureDropEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public override string ToString()
        {
            return string.Format("Header: {0}, Id: {1}, TypeId: {2}, Tile: {3}, Direction: {4}, IsRental: {5}, OwnerId: {6}, OwnerName: {7}",
                Packet.Header, Id, TypeId, Tile, Direction, IsRental, OwnerId, OwnerName);
        }
    }
}