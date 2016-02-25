using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostNavigateRoomEventArgs : DataInterceptedEventArgs
    {
        public int RoomId { get; }
        public string Password { get; }

        public HostNavigateRoomEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            RoomId = packet.ReadInteger();
            Password = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(RoomId)}: {RoomId}, {nameof(Password)}: {Password}";
    }
}