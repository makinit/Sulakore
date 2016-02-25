using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostDanceEventArgs : DataInterceptedEventArgs
    {
        public HDance Dance { get; }

        public HostDanceEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Dance = (HDance)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Dance)}: {Dance}";
    }
}