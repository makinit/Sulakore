using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreListView : ListView
    {
        public event EventHandler<ListViewItemSelectionChangedEventArgs> ItemSelected;
        protected virtual void OnItemSelected(ListViewItemSelectionChangedEventArgs e)
        {
            ItemSelected?.Invoke(this, e);
        }

        public event EventHandler ItemsDeselected;
        protected virtual void OnItemsDeselected(EventArgs e)
        {
            ItemsDeselected?.Invoke(this, e);
        }

        [DefaultValue(true)]
        public bool LockColumnWidth { get; set; }

        protected bool SuppressItemSelectedEvent { get; set; }

        public SKoreListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            GridLines = true;
            View = View.Details;
            MultiSelect = false;
            FullRowSelect = true;
            HideSelection = false;
            LockColumnWidth = true;
            ShowItemToolTips = true;
            UseCompatibleStateImageBehavior = false;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        public void ClearItems()
        {
            var items = new ListViewItem[Items.Count];
            Items.CopyTo(items, 0);

            ClearItems(items);
        }
        protected virtual void ClearItems(IEnumerable<ListViewItem> items)
        {
            foreach (ListViewItem item in items)
                RemoveItem(item);
        }

        public void RemoveSelectedItem()
        {
            if (SelectedItems.Count < 1) return;
            RemoveItem(SelectedItems[0]);
        }
        protected virtual void RemoveItem(ListViewItem item)
        {
            int index = item.Index;
            bool selectNext = Items.Count - 1 > 0;

            Items.RemoveAt(index);
            if (selectNext)
            {
                if (index >= Items.Count)
                    index = Items.Count - 1;

                item = Items[index];
                item.Selected = true;
                OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));

                EnsureVisible(item.Index);
            }
            else OnItemsDeselected(EventArgs.Empty);
        }

        public void MoveSelectedItemUp()
        {
            if (SelectedItems.Count < 1) return;
            MoveItemUp(SelectedItems[0]);
        }
        protected virtual void MoveItemUp(ListViewItem item)
        {
            int oldIndex = item.Index;
            if (oldIndex < 1) return;

            BeginUpdate();
            Items.RemoveAt(oldIndex);
            Items.Insert(oldIndex - 1, item);
            EndUpdate();

            item.Selected = true;
            OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));

            int index = item.Index;
            EnsureVisible(index <= 4 ? 0 : index - 4);
        }

        public void MoveSelectedItemDown()
        {
            if (SelectedItems.Count < 1) return;
            MoveItemDown(SelectedItems[0]);
        }
        protected virtual void MoveItemDown(ListViewItem item)
        {
            int oldIndex = item.Index;
            if (oldIndex == Items.Count - 1) return;

            BeginUpdate();
            Items.RemoveAt(oldIndex);
            Items.Insert(oldIndex + 1, item);
            EndUpdate();

            item.Selected = true;
            OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));

            int index = item.Index;
            EnsureVisible(index + 4 >= Items.Count ? Items.Count - 1 : index + 4);
        }

        public void FocusAdd(ListViewItem item)
        {
            Add(item);
            item.Selected = true;
        }
        public ListViewItem FocusAdd(params object[] items)
        {
            ListViewItem item = Add(items);
            item.Selected = true;

            return item;
        }

        public void Add(ListViewItem item)
        {
            Focus();
            Items.Add(item);

            if (!SuppressItemSelectedEvent)
                OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.IndentCount, true));

            item.EnsureVisible();
        }
        public ListViewItem Add(params object[] items)
        {
            string[] stringItems = items
                .Select(i => i.ToString()).ToArray();

            var item = new ListViewItem(stringItems);
            Add(item);

            return item;
        }

        public ListViewItem GetSelectedItem()
        {
            return SelectedItems.Count > 0 ?
                SelectedItems[0] : null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            ListViewItem item = GetItemAt(e.X, e.Y);
            if (item != null)
            {
                item.Selected = true;
                OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));
            }
            else
            {
                if (SelectedItems.Count > 0)
                    SelectedItems[0].Selected = false;

                OnItemsDeselected(EventArgs.Empty);
            }
        }
        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            if (LockColumnWidth && !DesignMode)
            {
                e.Cancel = true;
                e.NewWidth = Columns[e.ColumnIndex].Width;
            }
            base.OnColumnWidthChanging(e);
        }
    }
}