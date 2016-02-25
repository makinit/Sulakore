using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostWalkEventArgs : DataInterceptedEventArgs
    {
        public HPoint Tile { get; }

        public HostWalkEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Tile = new HPoint(packet.ReadInteger(0),
                packet.ReadInteger(4));
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Tile)}: {Tile}";
    }
}