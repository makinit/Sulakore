using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostUpdateMottoEventArgs : InterceptedEventArgs
    {
        public string Motto { get; }

        public HostUpdateMottoEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Motto = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Motto)}: {Motto}";
    }
}