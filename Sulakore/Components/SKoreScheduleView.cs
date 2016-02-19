using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    public class SKoreScheduleView : SKoreListView
    {
        private readonly Dictionary<HSchedule, ListViewItem> _items;
        private readonly Action<HSchedule, ScheduleTickEventArgs, bool> _updateItem;

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

                if (e.Interval > 0 && schedule.Interval != e.Interval ||
                    oldpacket != newPacket && !e.Packet.IsCorrupted)
                {
                    isDirty = true;
                }
            }
            catch { e.Cancel = true; }
            finally
            {
                if (isDirty || e.Cancel)
                    UpdateItem(schedule, e, isDirty);
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
                packet.Destination, interval, cycles, string.Empty);

            var schedule = new HSchedule(packet, interval, cycles);
            schedule.ScheduleTick += OnScheduleTick;

            _items.Add(schedule, item);

            item.Tag = schedule;
            item.Checked = autoStart;
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
        protected virtual void UpdateItem(HSchedule schedule, ScheduleTickEventArgs e, bool isDirty)
        {
            if (InvokeRequired) BeginInvoke(_updateItem, schedule, e, isDirty);
            else
            {
                if (e.Cancel)
                    _items[schedule].Checked = false;

                if (isDirty)
                {
                    schedule.Packet = e.Packet;
                    schedule.Interval = e.Interval;

                    ListViewItem item = _items[schedule];
                    item.SubItems[0].Text = e.Packet.ToString();
                    item.SubItems[1].Text = e.Packet.Destination.ToString();
                    item.SubItems[2].Text = e.Interval.ToString();
                    item.SubItems[3].Text = e.Cycles.ToString();
                }
            }
        }
    }
}