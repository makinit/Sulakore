using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerGestureEventArgs : DataInterceptedEventArgs, IHEntity
    {
        public int Index { get; }
        public HGesture Gesture { get; }

        public PlayerGestureEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Index = packet.ReadInteger();
            Gesture = (HGesture)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Index)}: {Index}, {nameof(Gesture)}: {Gesture}";
    }
}