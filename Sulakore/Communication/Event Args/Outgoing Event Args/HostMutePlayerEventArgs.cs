using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostMutePlayerEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public int RoomId { get; }
        public int Minutes { get; }

        public HostMutePlayerEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            RoomId = packet.ReadInteger();
            Minutes = packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(RoomId)}: {RoomId}, {nameof(Minutes)}: {Minutes}";
    }
}