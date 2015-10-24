using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostRaiseSignEventArgs : InterceptedEventArgs
    {
        public HSign Sign { get; }

        public HostRaiseSignEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Sign = (HSign)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Sign)}: {Sign}";
    }
}