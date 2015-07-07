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
    public class HostUpdateStanceEventArgs : InterceptedEventArgs
    {
        public HStance Stance { get; private set; }

        public HostUpdateStanceEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public HostUpdateStanceEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public HostUpdateStanceEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public HostUpdateStanceEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Stance = (HStance)packet.ReadInteger();
        }
        public HostUpdateStanceEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public override string ToString()
        {
            return string.Format("Header: {0}, Stance: {1}",
                Packet.Header, Stance);
        }
    }
}