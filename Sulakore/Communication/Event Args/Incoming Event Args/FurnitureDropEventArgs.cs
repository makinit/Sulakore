using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureDropEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public int TypeId { get; }
        public int OwnerId { get; }
        public HPoint Tile { get; }
        public bool IsRental { get; }
        public string OwnerName { get; }
        public HDirection Direction { get; }

        public FurnitureDropEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = Packet.ReadInteger();
            TypeId = Packet.ReadInteger();

            int x = Packet.ReadInteger();
            int y = Packet.ReadInteger();

            Direction = (HDirection)Packet.ReadInteger();
            Tile = new HPoint(x, y, double.Parse(Packet.ReadString()));

            Packet.ReadString();
            Packet.ReadInteger();
            Packet.ReadInteger();
            Packet.ReadString();

            IsRental = (Packet.ReadInteger() != 1);
            Packet.ReadInteger();

            OwnerId = Packet.ReadInteger();
            OwnerName = Packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(TypeId)}: {TypeId}, {nameof(Tile)}: {Tile}, {nameof(Direction)}: {Direction}, " +
            $"{nameof(IsRental)}: {IsRental}, {nameof(OwnerId)}: {OwnerId}, {nameof(OwnerName)}: {OwnerName}";
    }
}