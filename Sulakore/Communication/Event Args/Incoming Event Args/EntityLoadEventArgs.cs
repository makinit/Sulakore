using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class EntityLoadEventArgs : DataInterceptedEventArgs, IReadOnlyList<HEntity>
    {
        private readonly IReadOnlyList<HEntity> _entityLoadList;

        public int Count => _entityLoadList.Count;
        public HEntity this[int index] => _entityLoadList[index];

        public EntityLoadEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
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