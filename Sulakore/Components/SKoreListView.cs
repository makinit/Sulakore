using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreListView : ListView
    {
        private bool _lastSelectionState;
        private ListViewItem _previouslySelectedItem;

        /// <summary>
        /// Occurs when an item's selection state differs from the previous state.
        /// </summary>
        public event EventHandler ItemSelectionStateChanged;
        protected virtual void OnItemSelectionStateChanged(EventArgs e)
        {
            if (_lastSelectionState != HasSelectedItem)
            {
                _lastSelectionState = HasSelectedItem;
                ItemSelectionStateChanged?.Invoke(this, e);
            }

            if (!_lastSelectionState)
                _previouslySelectedItem = null;

            OnItemSelected(e);
        }

        /// <summary>
        /// Occurs when an item has been selected for the first time.
        /// </summary>
        public event EventHandler ItemSelected;
        protected virtual void OnItemSelected(EventArgs e)
        {
            if (HasSelectedItem &&
                (SelectedItem != _previouslySelectedItem))
            {
                _previouslySelectedItem = SelectedItem;
                ItemSelected?.Invoke(this, e);
            }
        }

        [DefaultValue(true)]
        public bool LockColumnWidth { get; set; }

        [Browsable(false)]
        public bool HasSelectedItem => (SelectedItems.Count > 0);

        [Browsable(false)]
        public ListViewItem SelectedItem =>
            (HasSelectedItem ? SelectedItems[0] : null);

        [Browsable(false)]
        public bool CanMoveSelectedItemUp
        {
            get
            {
                if (!HasSelectedItem) return false;
                return (SelectedItem.Index >= 1);
            }
        }

        [Browsable(false)]
        public bool CanMoveSelectedItemDown
        {
            get
            {
                if (!HasSelectedItem) return false;
                return (SelectedItem.Index != (Items.Count - 1));
            }
        }

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

        public virtual void ClearItems()
        {
            Items.Clear();
            OnItemSelectionStateChanged(EventArgs.Empty);
        }

        public void RemoveSelectedItem()
        {
            if (HasSelectedItem)
                RemoveItem(SelectedItem);
        }
        protected virtual void RemoveItem(ListViewItem item)
        {
            int index = item.Index;
            bool selectNext = (Items.Count - 1 > 0);

            Items.RemoveAt(index);
            if (selectNext)
            {
                if (index >= Items.Count)
                    index = Items.Count - 1;

                item = Items[index];
                item.Selected = true;
                EnsureVisible(item.Index);
            }
            OnItemSelectionStateChanged(EventArgs.Empty);
        }

        public void MoveSelectedItemUp()
        {
            if (HasSelectedItem)
                MoveItemUp(SelectedItem);
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
            OnItemSelectionStateChanged(EventArgs.Empty);

            int index = item.Index;
            EnsureVisible(index <= 4 ? 0 : index - 4);
        }

        public void MoveSelectedItemDown()
        {
            if (HasSelectedItem)
                MoveItemDown(SelectedItem);
        }
        protected virtual void MoveItemDown(ListViewItem item)
        {
            int oldIndex = item.Index;
            if (oldIndex == (Items.Count - 1)) return;

            BeginUpdate();
            Items.RemoveAt(oldIndex);
            Items.Insert(oldIndex + 1, item);
            EndUpdate();

            item.Selected = true;
            OnItemSelectionStateChanged(EventArgs.Empty);

            int index = item.Index;
            EnsureVisible(index + 4 >= Items.Count ? Items.Count - 1 : index + 4);
        }

        public void FocusAdd(ListViewItem item)
        {
            Add(item);
            item.Selected = true;
            OnItemSelectionStateChanged(EventArgs.Empty);
        }
        public ListViewItem FocusAdd(params object[] items)
        {
            ListViewItem item = Add(items);
            item.Selected = true;

            OnItemSelectionStateChanged(EventArgs.Empty);
            return item;
        }

        public void Add(ListViewItem item)
        {
            Focus();
            Items.Add(item);
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

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnItemSelectionStateChanged(EventArgs.Empty);
            base.OnMouseUp(e);
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