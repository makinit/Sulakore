using System;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreScheduleView : SKoreListView
    {
        private readonly IList<System.Timers.Timer> _runningTimers;
        private readonly IDictionary<ListViewItem, System.Timers.Timer> _timers;
        private readonly IDictionary<System.Timers.Timer, Tuple<HMessage, int>> _timerItems;

        public event EventHandler<ScheduleTickEventArgs> ScheduleTick;
        protected virtual void OnScheduleTick(ScheduleTickEventArgs e)
        {
            EventHandler<ScheduleTickEventArgs> handler = ScheduleTick;
            if (handler != null)
            {
                try { handler(this, e); }
                catch { e.Cancel = true; }
            }
        }

        [DefaultValue(true)]
        public bool AutoStart { get; set; }

        [Browsable(false)]
        public int SchedulesRunning
        {
            get { return _runningTimers.Count; }
        }

        [DefaultValue(false)]
        public bool IsSynchronized { get; set; }

        public SKoreScheduleView()
        {
            _runningTimers = new List<System.Timers.Timer>();
            _timers = new Dictionary<ListViewItem, System.Timers.Timer>();
            _timerItems = new Dictionary<System.Timers.Timer, Tuple<HMessage, int>>();

            AutoStart = true;
            CheckBoxes = true;
        }

        public void RemoveSelected()
        {
            ListViewItem item = GetSelectedItem();
            if (item == null) return;

            System.Timers.Timer timer = _timers[item];
            try
            {
                timer.Elapsed -= Elapsed;

                _timers.Remove(item);
                _timerItems.Remove(timer);

                if (_runningTimers.Contains(timer))
                    _runningTimers.Remove(timer);
            }
            finally { timer.Dispose(); }
        }
        public void AddSchedule(HMessage packet, int interval, int burst)
        {
            var timer = new System.Timers.Timer(interval);
            timer.Elapsed += Elapsed;

            if (IsSynchronized)
                timer.SynchronizingObject = FindForm();

            ListViewItem item = FocusAdd(packet, packet.Destination,
                interval, AutoStart ? "Running" : "Stopped");

            var timerTuple = new Tuple<HMessage, int>(packet, burst);
            _timers.Add(item, timer);
            _timerItems.Add(timer, timerTuple);

            item.Checked = AutoStart;
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            if (Monitor.TryEnter(timer))
            {
                try
                {
                    timer.Stop();

                    Tuple<HMessage, int> timerItems = null;

                    if (_timerItems.ContainsKey(timer))
                        timerItems = _timerItems[timer];
                    else return;

                    int tmpBurst = timerItems.Item2;

                    bool shouldStart = true;
                    for (int i = 0; i < tmpBurst && _runningTimers.Contains(timer); i++)
                    {
                        var args = new ScheduleTickEventArgs(timerItems.Item1);
                        OnScheduleTick(args);

                        if (args.Cancel)
                        {
                            shouldStart = false;
                            break;
                        }
                    }

                    if (shouldStart && _runningTimers.Contains(timer))
                        timer.Start();
                }
                finally { Monitor.Exit(timer); }
            }
            else return;
        }
        protected override void OnItemChecked(ItemCheckedEventArgs e)
        {
            if (_timers.ContainsKey(e.Item))
            {
                System.Timers.Timer timer = _timers[e.Item];
                bool isChecked = e.Item.Checked;

                e.Item.SubItems[3].Text =
                    isChecked ? "Running" : "Stopped";

                if (isChecked)
                {
                    _runningTimers.Add(timer);
                    timer.Start();
                }
                else
                {
                    _runningTimers.Remove(timer);
                    timer.Stop();
                }
            }
            base.OnItemChecked(e);
        }
    }
}