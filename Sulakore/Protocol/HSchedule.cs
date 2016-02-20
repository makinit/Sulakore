using System;
using System.Timers;
using System.ComponentModel;

using Sulakore.Components;

namespace Sulakore.Protocol
{
    public class HSchedule : IDisposable
    {
        private int _currentCycle;
        private readonly Timer _ticker;

        public event EventHandler<ScheduleTickEventArgs> ScheduleTick;
        protected virtual void OnScheduleTick(ScheduleTickEventArgs e)
        {
            try
            {
                if (Cycles != 0 && _currentCycle >= Cycles)
                {
                    IsRunning = false;
                    e.Cancel = true;
                }
                ScheduleTick?.Invoke(this, e);
            }
            catch { e.Cancel = true; }
            finally
            {
                if (e.Cancel)
                    IsRunning = false;
            }
        }
        protected void RaiseOnScheduleTick(HSchedule schedule, int currentCycle)
        {
            if (ScheduleTick != null)
            {
                OnScheduleTick(
                    new ScheduleTickEventArgs(schedule, currentCycle));
            }
        }

        public int Interval
        {
            get { return (int)_ticker.Interval; }
            set
            {
                if (value > 0)
                    _ticker.Interval = value;
            }
        }
        public bool IsRunning
        {
            get { return _ticker.Enabled; }
            set
            {
                if (_ticker.Enabled != value)
                {
                    _currentCycle = 0;
                    _ticker.Enabled = value;
                }
            }
        }
        public ISynchronizeInvoke SynchronizingObject
        {
            get { return _ticker.SynchronizingObject; }
            set { _ticker.SynchronizingObject = value; }
        }

        private int _cycles;
        public int Cycles
        {
            get { return _cycles; }
            set
            {
                if (_cycles >= 0)
                    _cycles = value;
            }
        }

        public HMessage Packet { get; set; }

        public bool IsDisposed { get; private set; }

        public HSchedule(HMessage packet, int interval, int cycles)
        {
            _ticker = new Timer(interval);
            _ticker.Elapsed += Elapsed;

            Packet = packet;
            Cycles = cycles;
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_ticker)
            {
                if (!IsRunning) return;
                if (Cycles != 0) _currentCycle++;
                RaiseOnScheduleTick(this, _currentCycle);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                IsRunning = false;
                _ticker.Dispose();
            }
            SKore.Unsubscribe(ref ScheduleTick);
            IsDisposed = true;
        }
    }
}