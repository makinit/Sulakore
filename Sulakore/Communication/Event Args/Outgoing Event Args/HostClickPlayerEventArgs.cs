using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostClickPlayerEventArgs : DataInterceptedEventArgs
    {
        public int Id { get; }
        public HPoint Tile { get; }

        public HostClickPlayerEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Id = packet.ReadInteger();
            Tile = new HPoint(packet.ReadInteger(), packet.ReadInteger());
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Id)}: {Id}, {nameof(Tile)}: {Tile}";
    }
}