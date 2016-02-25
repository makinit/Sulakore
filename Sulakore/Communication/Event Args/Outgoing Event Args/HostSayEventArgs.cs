using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostSayEventArgs : DataInterceptedEventArgs
    {
        public HTheme Theme { get; }
        public string Message { get; }

        public HostSayEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Message = packet.ReadString();

            // TODO: Find the chunks before Theme and read them.
            Theme = (HTheme)packet.ReadInteger(packet.Length - 10);
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Message)}: {Message}, {nameof(Theme)}: {Theme}";
    }
}