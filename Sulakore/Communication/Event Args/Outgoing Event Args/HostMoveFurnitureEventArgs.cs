using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostMoveFurnitureEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public HPoint Tile { get; }
        public HDirection Direction { get; }

        public HostMoveFurnitureEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            Tile = new HPoint(packet.ReadInteger(), packet.ReadInteger());
            Direction = (HDirection)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(Tile)}: {Tile}, {nameof(Direction)}: {Direction}";
    }
}