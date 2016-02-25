using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class EntityActionEventArgs : DataInterceptedEventArgs, IReadOnlyList<HEntityAction>
    {
        private readonly IReadOnlyList<HEntityAction> _entityActionList;

        public int Count => _entityActionList.Count;
        public HEntityAction this[int index] => _entityActionList[index];

        public EntityActionEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            _entityActionList = HEntityAction.Parse(packet);
        }

        public IEnumerator<HEntityAction> GetEnumerator() =>
            _entityActionList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_entityActionList).GetEnumerator();

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Count)}: {Count}";
    }
}