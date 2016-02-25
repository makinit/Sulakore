using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostUpdateMottoEventArgs : DataInterceptedEventArgs
    {
        public string Motto { get; }

        public HostUpdateMottoEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Motto = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Motto)}: {Motto}";
    }
}