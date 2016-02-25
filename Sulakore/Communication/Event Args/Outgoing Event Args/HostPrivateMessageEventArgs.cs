using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostPrivateMessageEventArgs : DataInterceptedEventArgs
    {
        public int Id { get; }
        public string Message { get; }

        public HostPrivateMessageEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Id = packet.ReadInteger();
            Message = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Id)}: {Id}, {nameof(Message)}: {Message}";
    }
}