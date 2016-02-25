using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostRaiseSignEventArgs : DataInterceptedEventArgs
    {
        public HSign Sign { get; }

        public HostRaiseSignEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Sign = (HSign)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Sign)}: {Sign}";
    }
}