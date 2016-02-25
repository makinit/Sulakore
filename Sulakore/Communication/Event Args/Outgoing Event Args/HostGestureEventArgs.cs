using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostGestureEventArgs : DataInterceptedEventArgs
    {
        public HGesture Gesture { get; }

        public HostGestureEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Gesture = (HGesture)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Gesture)}: {Gesture}";
    }
}