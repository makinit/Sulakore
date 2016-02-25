using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostShoutEventArgs : DataInterceptedEventArgs
    {
        public HTheme Theme { get; }
        public string Message { get; }

        public HostShoutEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Message = packet.ReadString();

            // TODO: Find the chunks before OwnerId and read them.
            Theme = (HTheme)packet.ReadInteger(packet.Length - 6);
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Message)}: {Message}, {nameof(Theme)}: {Theme}";
    }
}