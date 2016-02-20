﻿using System.ComponentModel;

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
            set { _schedule.Interval = value; }
        }
        public HMessage Packet
        {
            get { return _schedule.Packet; }
            set { _schedule.Packet = value; }
        }

        public ScheduleTickEventArgs(HSchedule schedule, int currentCycle)
        {
            _schedule = schedule;

            CurrentCycle = currentCycle;
        }
    }
}