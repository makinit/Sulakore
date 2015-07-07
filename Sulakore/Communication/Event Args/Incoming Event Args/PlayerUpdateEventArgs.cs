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
    public class PlayerUpdateEventArgs : InterceptedEventArgs, IHEntity
    {
        public int Index { get; private set; }
        public string Motto { get; private set; }
        public HGender Gender { get; private set; }
        public string FigureId { get; private set; }

        public PlayerUpdateEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public PlayerUpdateEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public PlayerUpdateEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public PlayerUpdateEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Index = packet.ReadInteger();
            FigureId = packet.ReadString();
            Gender = SKore.ToGender(packet.ReadString());
            Motto = packet.ReadString();
        }
        public PlayerUpdateEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public override string ToString()
        {
            return string.Format("Header: {0}, Index: {1}, FigureId: {2}, Gender: {3}, Motto: {4}",
                Packet.Header, Index, FigureId, Gender, Motto);
        }
    }
}