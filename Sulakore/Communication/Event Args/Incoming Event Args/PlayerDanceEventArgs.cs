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
    public class PlayerDanceEventArgs : InterceptedEventArgs, IHEntity
    {
        public int Index { get; private set; }
        public HDance Dance { get; private set; }

        public PlayerDanceEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public PlayerDanceEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public PlayerDanceEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public PlayerDanceEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Index = packet.ReadInteger();
            Dance = (HDance)packet.ReadInteger();
        }
        public PlayerDanceEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public override string ToString()
        {
            return string.Format("Header: {0}, Index: {1}, Dance: {2}",
                Packet.Header, Index, Dance);
        }
    }
}