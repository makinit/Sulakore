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

using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureMoveEventArgs : InterceptedEventArgs
    {
        public int Id { get; private set; }
        public int TypeId { get; private set; }
        public int OwnerId { get; private set; }
        public HPoint Tile { get; private set; }
        public HDirection Direction { get; private set; }

        public FurnitureMoveEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public FurnitureMoveEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public FurnitureMoveEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public FurnitureMoveEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            TypeId = packet.ReadInteger();

            int x = packet.ReadInteger();
            int y = packet.ReadInteger();

            Direction = (HDirection)packet.ReadInteger();
            Tile = new HPoint(x, y, double.Parse(packet.ReadString()));

            // TODO: Find the chunks before OwnerId and read them.
            OwnerId = packet.ReadInteger(packet.Length - 6);
        }
        public FurnitureMoveEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public override string ToString()
        {
            return string.Format("Header: {0}, Id: {1}, OwnerId: {2}, Tile: {3}, Direction: {4}",
                Packet.Header, Id, OwnerId, Tile, Direction);
        }
    }
}