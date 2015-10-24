using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostGestureEventArgs : InterceptedEventArgs
    {
        public HGesture Gesture { get; }

        public HostGestureEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Gesture = (HGesture)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Gesture)}: {Gesture}";
    }
}