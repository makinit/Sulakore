using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostPrivateMessageEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public string Message { get; }

        public HostPrivateMessageEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            Message = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Id)}: {Id}, {nameof(Message)}: {Message}";
    }
}