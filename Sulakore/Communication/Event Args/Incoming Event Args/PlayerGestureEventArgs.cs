using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerGestureEventArgs : InterceptedEventArgs, IHEntity
    {
        public int Index { get; }
        public HGesture Gesture { get; }

        public PlayerGestureEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Index = packet.ReadInteger();
            Gesture = (HGesture)packet.ReadInteger();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Index)}: {Index}, {nameof(Gesture)}: {Gesture}";
    }
}