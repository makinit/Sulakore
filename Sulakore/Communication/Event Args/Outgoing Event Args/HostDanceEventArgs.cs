using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostDanceEventArgs : InterceptedEventArgs
    {
        public HDance Dance { get; }

        public HostDanceEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Dance = (HDance)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Dance)}: {Dance}";
    }
}