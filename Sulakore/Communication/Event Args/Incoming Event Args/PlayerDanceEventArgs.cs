using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerDanceEventArgs : InterceptedEventArgs, IHEntity
    {
        public int Index { get; }
        public HDance Dance { get; }

        public PlayerDanceEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Index = packet.ReadInteger();
            Dance = (HDance)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Index)}: {Index}, {nameof(Dance)}: {Dance}";
    }
}