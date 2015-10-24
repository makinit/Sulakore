using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostExitRoomEventArgs : InterceptedEventArgs
    {
        public HostExitRoomEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        { }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}";
    }
}