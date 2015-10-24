using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostBanPlayerEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public HBan Ban { get; }
        public int RoomId { get; }

        public HostBanPlayerEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            RoomId = packet.ReadInteger();
            Ban = SKore.ToBan(packet.ReadString());
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(RoomId)}: {RoomId}, {nameof(Ban)}: {Ban}";
    }
}