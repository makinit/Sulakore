using System.ComponentModel;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    public class ScheduleTickEventArgs : CancelEventArgs
    {
        public int BurstLeft { get; }
        public int BurstCount { get; }
        public HMessage Packet { get; }
        public bool IsFinalBurst { get; }

        public ScheduleTickEventArgs(HMessage packet,
            int burstCount, int burstLeft, bool isFinalBurst)
        {
            Packet = packet;
            BurstLeft = burstLeft;
            BurstCount = burstCount;
            IsFinalBurst = isFinalBurst;
        }
    }
}