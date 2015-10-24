using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostKickPlayerEventArgs : InterceptedEventArgs
    {
        public int Id { get; }

        public HostKickPlayerEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}";
    }
}