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
        private readonly Dictionary<ListViewItem, HSchedule> _schedules;
        private readonly Dictionary<ListViewItem, string> _descriptions;

        public event EventHandler<ScheduleTickEventArgs> ScheduleTick;
        protected virtual void OnScheduleTick(object sender, ScheduleTickEventArgs e)
        {
            EventHandler<ScheduleTickEventArgs> handler = ScheduleTick;
            if (handler != null)
            {
                try { handler(sender, e); }
                catch { e.Cancel = true; }
                finally
                {
                    if (e.Cancel)
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            ListViewItem item = _items[(HSchedule)sender];
                            item.SubItems[4].Text = "Stopped";
                            item.Checked = false;
                        }));
                    }
                }
            }
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool AutoStart { get; set; }

        [Browsable(false)]
        public int Running
        {
            get { return _items.Keys.Count(x => x.IsRunning); }
        }

        public SKoreScheduleView()
        {
            _items = new Dictionary<HSchedule, ListViewItem>();
            _descriptions = new Dictionary<ListViewItem, string>();
            _schedules = new Dictionary<ListViewItem, HSchedule>();

            AutoStart = true;
            CheckBoxes = true;
        }

        protected override void RemoveItem(ListViewItem listViewItem)
        {
            if (_schedules.ContainsKey(listViewItem))
            {
                HSchedule schedule = _schedules[listViewItem];
                _items.Remove(schedule);

                schedule.Dispose();
                _schedules.Remove(listViewItem);

                _descriptions.Remove(listViewItem);
            }

            base.RemoveItem(listViewItem);
        }
        public void AddSchedule(HMessage packet, int burst, int interval, string description)
        {
            if (packet.IsCorrupted)
                throw new Exception("Corrupted Packet: " + packet);

            var item = new ListViewItem(new[] { packet.ToString(),
                packet.Destination.ToString(), burst.ToString(), interval.ToString(), AutoStart ? "Running" : "Stopped" });

            var schedule = new HSchedule(packet, interval, burst);
            schedule.ScheduleTick += OnScheduleTick;

            _items.Add(schedule, item);
            _schedules.Add(item, schedule);
            _descriptions.Add(item, description);

            item.Checked = AutoStart;
            item.ToolTipText = description;

            FocusAdd(item);
        }

        public void StopAllSchedules()
        {
            foreach (ListViewItem item in _schedules.Keys)
                item.Checked = false;
        }
        public void StartAllSchedules()
        {
            foreach (ListViewItem item in _schedules.Keys)
                item.Checked = true;
        }

        public int GetItemBurst()
        {
            return SelectedItems.Count > 0 ?
                _schedules[SelectedItems[0]].Burst : 0;
        }
        public void SetItemBurst(int burst)
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            _schedules[item].Burst = burst;
            item.SubItems[2].Text = burst.ToString();
        }

        public int GetItemInterval()
        {
            return SelectedItems.Count > 0 ?
                _schedules[SelectedItems[0]].Interval : 0;
        }
        public void SetItemInterval(int interval)
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            _schedules[item].Interval = interval;
            item.SubItems[2].Text = interval.ToString();
        }

        public string GetItemDescription()
        {
            return SelectedItems.Count > 0 ?
                _descriptions[SelectedItems[0]] : string.Empty;
        }
        public void SetItemDescription(string description)
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            item.ToolTipText = description;
            _descriptions[item] = description;
        }

        public HMessage GetItemPacket()
        {
            return SelectedItems.Count > 0 ?
                _schedules[SelectedItems[0]].Packet : null;
        }
        public void SetItemPacket(HMessage packet)
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            _schedules[item].Packet = packet;
            item.SubItems[0].Text = packet.ToString();
            item.SubItems[1].Text = packet.Destination.ToString();
        }

        public HDestination GetItemDestination()
        {
            return SelectedItems.Count > 0 ?
                _schedules[SelectedItems[0]].Packet.Destination : HDestination.Client;
        }
        public void SetItemDestination(HDestination destination)
        {
            if (SelectedItems.Count < 1) return;

            ListViewItem item = SelectedItems[0];
            _schedules[item].Packet.Destination = destination;
            item.SubItems[1].Text = destination.ToString();
        }

        protected override void OnItemChecked(ItemCheckedEventArgs e)
        {
            if (!_schedules.ContainsKey(e.Item)) return;

            HSchedule schedule = _schedules[e.Item];
            e.Item.SubItems[4].Text = e.Item.Checked ? "Running" : "Stopped";

            if (e.Item.Checked) schedule.Start();
            else if (schedule.IsRunning) schedule.Stop();

            base.OnItemChecked(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                foreach (HSchedule schedule in _schedules.Values)
                    schedule.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}