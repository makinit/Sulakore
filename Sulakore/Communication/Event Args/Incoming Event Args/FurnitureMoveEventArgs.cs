using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureMoveEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public int TypeId { get; }
        public int OwnerId { get; }
        public HPoint Tile { get; }
        public HDirection Direction { get; }

        public FurnitureMoveEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            TypeId = packet.ReadInteger();

            int x = packet.ReadInteger();
            int y = packet.ReadInteger();

            Direction = (HDirection)packet.ReadInteger();
            Tile = new HPoint(x, y, double.Parse(packet.ReadString()));

            // TODO: Find the chunks before OwnerId and read them.
            OwnerId = packet.ReadInteger(packet.Length - 6);
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(OwnerId)}: {OwnerId}, {nameof(Tile)}: {Tile}, {nameof(Direction)}: {Direction}";
    }
}