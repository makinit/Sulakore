using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerDanceEventArgs : DataInterceptedEventArgs, IHEntity
    {
        public int Index { get; }
        public HDance Dance { get; }

        public PlayerDanceEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Index = packet.ReadInteger();
            Dance = (HDance)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Index)}: {Index}, {nameof(Dance)}: {Dance}";
    }
}