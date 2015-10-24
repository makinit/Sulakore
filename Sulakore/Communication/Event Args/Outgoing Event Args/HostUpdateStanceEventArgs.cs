using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostUpdateStanceEventArgs : InterceptedEventArgs
    {
        public HStance Stance { get; }

        public HostUpdateStanceEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Stance = (HStance)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Stance)}: {Stance}";
    }
}