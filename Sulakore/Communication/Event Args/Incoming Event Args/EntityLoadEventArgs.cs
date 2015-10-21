/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
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
    public class EntityLoadEventArgs : InterceptedEventArgs, IReadOnlyList<HEntity>
    {
        private readonly IReadOnlyList<HEntity> _entityLoadList;

        public int Count => _entityLoadList.Count;
        public HEntity this[int index] => _entityLoadList[index];

        public EntityLoadEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            _entityLoadList = HEntity.Parse(packet);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_entityLoadList).GetEnumerator();

        public IEnumerator<HEntity> GetEnumerator() =>
            _entityLoadList.GetEnumerator();

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Count)}: {Count}";
    }
}