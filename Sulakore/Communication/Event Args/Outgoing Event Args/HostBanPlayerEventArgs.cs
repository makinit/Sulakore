using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostBanPlayerEventArgs : DataInterceptedEventArgs
    {
        public int Id { get; }
        public HBan Ban { get; }
        public int RoomId { get; }

        public HostBanPlayerEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
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