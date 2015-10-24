using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureLoadEventArgs : InterceptedEventArgs, IReadOnlyList<HFurniture>
    {
        private readonly IReadOnlyList<HFurniture> _furnitureLoadList;

        public int Count => _furnitureLoadList.Count;
        public HFurniture this[int index] => _furnitureLoadList[index];

        public FurnitureLoadEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            _furnitureLoadList = HFurniture.Parse(packet);
        }

        public IEnumerator<HFurniture> GetEnumerator() =>
            _furnitureLoadList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_furnitureLoadList).GetEnumerator();

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Count)}: {Count}";
    }
}