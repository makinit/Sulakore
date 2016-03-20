using System;
using System.Timers;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreScheduleView : SKoreListView
    {
        private readonly Action<HSchedule> _updateItem;
        private readonly Dictionary<HSchedule, ListViewItem> _items;

        public event EventHandler<ScheduleTickEventArgs> ScheduleTick;
        protected virtual void OnScheduleTick(object sender, ScheduleTickEventArgs e)
        {
            bool isDirty = false;
            var schedule = (HSchedule)sender;
            try
            {
                string oldpacket = e.Packet.ToString();
                ScheduleTick?.Invoke(this, e);
                string newPacket = e.Packet.ToString();

                if (oldpacket != newPacket ||
                    e.Interval > 0 && schedule.Interval != e.Interval)
                {
                    isDirty = true;
                }
            }
            catch { e.Cancel = true; }
            finally
            {
                schedule.IsRunning = !e.Cancel;

                if (isDirty || e.Cancel)
                    UpdateItem(schedule);
            }
        }

        protected HSchedule SelectedSchedule
        {
            get
            {
                if (!HasSelectedItem)
                    return null;

                var schedule =
                    (SelectedItem.Tag as HSchedule);

                return schedule;
            }
        }

        public int SelectedCycles
        {
            get { return SelectedSchedule?.Cycles ?? -1; }
            set
            {
                if (!HasSelectedItem) return;
                if (SelectedSchedule.Cycles != value)
                {
                    SelectedSchedule.Cycles = value;
                    UpdateItem(SelectedSchedule);
                }
            }
        }
        public int SelectedInterval
        {
            get { return SelectedSchedule?.Interval ?? -1; }
            set
            {
                if (!HasSelectedItem) return;
                if (SelectedSchedule.Interval != value)
                {
                    SelectedSchedule.Interval = value;
                    UpdateItem(SelectedSchedule);
                }
            }
        }
        public string SelectedPacket
        {
            get { return SelectedSchedule?.Packet.ToString(); }
            set
            {
                if (!HasSelectedItem) return;
                if (SelectedSchedule.Packet.ToString() != value)
                {
                    var packet = new HMessage(value, SelectedDestination);
                    if (packet.IsCorrupted) return;

                    SelectedSchedule.Packet = packet;
                    UpdateItem(SelectedSchedule);
                }
            }
        }
        public HDestination SelectedDestination
        {
            get
            {
                return (SelectedSchedule?
                    .Packet.Destination ?? HDestination.Client);
            }
            set
            {
                if (!HasSelectedItem) return;
                if (SelectedSchedule.Packet.Destination != value)
                {
                    SelectedSchedule
                        .Packet.Destination = value;

                    UpdateItem(SelectedSchedule);
                }
            }
        }

        public SKoreScheduleView()
        {
            _updateItem = UpdateItem;
            _items = new Dictionary<HSchedule, ListViewItem>();

            CheckBoxes = true;
        }

        public void AddSchedule(HMessage packet, int interval, int cycles, bool autoStart)
        {
            ListViewItem item = AddFocusedItem(packet,
                packet.Destination, interval, cycles, "Stopped");

            var schedule = new HSchedule(packet, interval, cycles);
            schedule.ScheduleTick += OnScheduleTick;

            _items.Add(schedule, item);

            item.Tag = schedule;
            item.Checked = autoStart;
        }

        protected virtual void UpdateItem(HSchedule schedule)
        {
            if (InvokeRequired) BeginInvoke(_updateItem, schedule);
            else
            {
                ListViewItem item = _items[schedule];

                if (item.Checked != schedule.IsRunning)
                    item.Checked = schedule.IsRunning;

                item.SubItems[0].Text = schedule.Packet.ToString();
                item.SubItems[1].Text = schedule.Packet.Destination.ToString();
                item.SubItems[2].Text = schedule.Interval.ToString();
                item.SubItems[3].Text = schedule.Cycles.ToString();
            }
        }
        protected override void OnItemChecked(ItemCheckedEventArgs e)
        {
            var schedule = (e.Item.Tag as HSchedule);
            if (schedule != null)
            {
                bool isRunning = e.Item.Checked;
                schedule.IsRunning = isRunning;

                e.Item.SubItems[4].Text =
                    (isRunning ? "Running" : "Stopped");
            }
            base.OnItemChecked(e);
        }
        protected override void RemoveItem(ListViewItem item, bool selectNext)
        {
            var schedule = (item.Tag as HSchedule);
            if (schedule != null)
            {
                if (_items.ContainsKey(schedule))
                    _items.Remove(schedule);

                schedule.Dispose();
            }
            base.RemoveItem(item, selectNext);
        }

        public sealed class HSchedule : IDisposable
        {
            private int _currentCycle;
            private readonly System.Timers.Timer _ticker;

            public event EventHandler<ScheduleTickEventArgs> ScheduleTick;
            private void OnScheduleTick(ScheduleTickEventArgs e)
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
            private void RaiseOnScheduleTick(HSchedule schedule, int currentCycle)
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
                _ticker = new System.Timers.Timer(interval);
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
            private void Dispose(bool disposing)
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
}