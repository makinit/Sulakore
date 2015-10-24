using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostWalkEventArgs : InterceptedEventArgs
    {
        public HPoint Tile { get; }

        public HostWalkEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Tile = new HPoint(packet.ReadInteger(0),
                packet.ReadInteger(4));
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Tile)}: {Tile}";
    }
}