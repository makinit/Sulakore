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
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class EntityActionEventArgs : InterceptedEventArgs, IReadOnlyList<HEntityAction>
    {
        private readonly IReadOnlyList<HEntityAction> _entityActionList;

        public int Count
        {
            get { return _entityActionList.Count; }
        }
        public HEntityAction this[int index]
        {
            get { return _entityActionList[index]; }
        }

        public EntityActionEventArgs(HMessage packet)
            : this(null, -1, packet)
        { }
        public EntityActionEventArgs(int step, HMessage packet)
            : this(null, step, packet)
        { }
        public EntityActionEventArgs(int step, byte[] data, HDestination destination)
            : this(null, step, new HMessage(data, destination))
        { }
        public EntityActionEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            _entityActionList = HEntityAction.Parse(packet);
        }
        public EntityActionEventArgs(Func<Task> continuation, int step, byte[] data, HDestination destination)
            : this(continuation, step, new HMessage(data, destination))
        { }

        public IEnumerator<HEntityAction> GetEnumerator()
        {
            return _entityActionList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_entityActionList).GetEnumerator();
        }
        public override string ToString()
        {
            return string.Format("Header: {0}, Count: {1}", Packet.Header, Count);
        }
    }
}