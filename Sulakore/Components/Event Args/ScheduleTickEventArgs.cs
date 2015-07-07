
using System.ComponentModel;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    public class ScheduleTickEventArgs : CancelEventArgs
    {
        public HMessage Packet { get; private set; }

        public ScheduleTickEventArgs(HMessage packet)
        {
            Packet = packet;
        }
    }
}