using System.ComponentModel;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    public class ScheduleTickEventArgs : CancelEventArgs
    {
        private readonly HSchedule _schedule;

        public int CurrentCycle { get; }
        public int Cycles => _schedule.Cycles;

        public int Interval
        {
            get { return _schedule.Interval; }
            set
            {
                if (value > 0)
                    _schedule.Interval = value;
            }
        }
        public HMessage Packet
        {
            get { return _schedule.Packet; }
            set
            {
                if (!value.IsCorrupted)
                    _schedule.Packet = value;
            }
        }

        public ScheduleTickEventArgs(HSchedule schedule, int currentCycle)
        {
            _schedule = schedule;

            CurrentCycle = currentCycle;
        }
    }
}