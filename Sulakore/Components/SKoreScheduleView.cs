/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
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
            try { ScheduleTick?.Invoke(this, e); }
            catch { e.Cancel = true; }
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
                    {
                        timerItems = _timerItems[timer];
                    }
                    else return;

                    bool shouldStart = true;
                    int tmpBurst = timerItems.Item2;
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

        public void StopAll()
        {
            var items = new ListViewItem[Items.Count];
            Items.CopyTo(items, 0);

            foreach (ListViewItem item in items)
            {
                if (item.Checked)
                    item.Checked = false;
            }
        }
        public void StartAll()
        {
            var items = new ListViewItem[Items.Count];
            Items.CopyTo(items, 0);

            foreach (ListViewItem item in items)
            {
                if (!item.Checked)
                    item.Checked = true;
            }
        }

        protected override void RemoveItem(ListViewItem item)
        {
            if (_timers.ContainsKey(item))
            {
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
            base.RemoveItem(item);
        }
        protected override void OnItemChecked(ItemCheckedEventArgs e)
        {
            if (_timers.ContainsKey(e.Item))
            {
                System.Timers.Timer timer = _timers[e.Item];
                bool isChecked = e.Item.Checked;

                e.Item.SubItems[4].Text =
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

        public void AddSchedule(HMessage packet, int interval, int burst)
        {
            var timer = new System.Timers.Timer(interval);
            timer.Elapsed += Elapsed;

            if (IsSynchronized)
                timer.SynchronizingObject = FindForm();

            ListViewItem item = FocusAdd(packet, packet.Destination,
                burst, interval, AutoStart ? "Running" : "Stopped");

            var timerTuple = new Tuple<HMessage, int>(packet, burst);
            _timers.Add(item, timer);
            _timerItems.Add(timer, timerTuple);

            item.Checked = AutoStart;
        }
    }
}