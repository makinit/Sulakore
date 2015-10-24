﻿using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreConstructView : SKoreListView
    {
        private readonly HMessage _packet;

        private const string CHUNK_TIP =
            "Type: {0}\nValue: {1}\nBlock Length: {2}\nEncoded: {3}";

        [DefaultValue(0)]
        public ushort Header
        {
            get { return _packet.Header; }
            set { _packet.Header = value; }
        }

        [Browsable(false)]
        public int Length => _packet.Length;

        [Browsable(false)]
        public IReadOnlyList<object> ValuesWritten => _packet.ValuesWritten;

        public SKoreConstructView()
        {
            _packet = new HMessage(0);
        }

        public void ReplaceItem(object chunk)
        {
            ListViewItem item = SelectedItems[0];
            _packet.ReplaceWritten(item.Index, chunk);
            ListViewItem.ListViewSubItemCollection subItems = item.SubItems;

            subItems[0].Text = chunk.GetType().Name
                .Replace("Int32", "Integer");

            byte[] data = HMessage.GetBytes(chunk);
            subItems[1].Text = chunk.ToString();
            subItems[2].Text = HMessage.ToString(data);

            item.ToolTipText = string.Format(CHUNK_TIP,
                subItems[0].Text, subItems[1].Text, data.Length, subItems[2].Text);
        }
        public void Write(params object[] values)
        {
            _packet.WriteObjects(values);
            try
            {
                BeginUpdate();
                ListViewItem item = null;
                SuppressItemSelectedEvent = true;

                foreach (object value in values)
                {
                    string valueString = value.ToString();
                    byte[] data = HMessage.GetBytes(value);
                    string encoded = HMessage.ToString(data);
                    string typeName = value.GetType().Name.Replace("Int32", "Integer");

                    item = FocusAdd(typeName, valueString, encoded);
                    item.ToolTipText = string.Format(CHUNK_TIP, typeName, valueString, data.Length, encoded);
                }

                SuppressItemSelectedEvent = false;
                OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));
            }
            finally { EndUpdate(); }
        }

        protected override void RemoveItem(ListViewItem listViewItem)
        {
            _packet.RemoveWritten(listViewItem.Index);
            base.RemoveItem(listViewItem);
        }
        protected override void MoveItemUp(ListViewItem listViewItem)
        {
            _packet.MoveWritten(listViewItem.Index, 1, false);
            base.MoveItemUp(listViewItem);
        }
        protected override void MoveItemDown(ListViewItem listViewItem)
        {
            _packet.MoveWritten(listViewItem.Index, 1, true);
            base.MoveItemDown(listViewItem);
        }

        public void ClearWritten()
        {
            Items.Clear();
            _packet.ClearWritten();
            OnItemsDeselected(EventArgs.Empty);
        }
        public byte[] GetBytes() => _packet.ToBytes();
        public string GetString() => _packet.ToString();
    }
}