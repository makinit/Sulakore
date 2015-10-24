using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostNavigateRoomEventArgs : InterceptedEventArgs
    {
        public int RoomId { get; }
        public string Password { get; }

        public HostNavigateRoomEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            RoomId = packet.ReadInteger();
            Password = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(RoomId)}: {RoomId}, {nameof(Password)}: {Password}";
    }
}