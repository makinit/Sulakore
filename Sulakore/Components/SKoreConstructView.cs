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
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreConstructView : SKoreListView
    {
        private const string CHUNK_TIP = "Type: {0}\nValue: {1}\nBlock Length: {2}\nEncoded: {3}";

        private readonly HMessage _packet;

        public ushort Header
        {
            get { return _packet.Header; }
            set { _packet.Header = value; }
        }
        public int Length => _packet.Length;
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

            byte[] data = HMessage.Encode(chunk);
            subItems[1].Text = chunk.ToString();
            subItems[2].Text = HMessage.ToString(data);

            item.ToolTipText = string.Format(CHUNK_TIP,
                subItems[0].Text, subItems[1].Text, data.Length, subItems[2].Text);
        }
        public void Write(params object[] chunks)
        {
            _packet.Write(chunks);
            try
            {
                BeginUpdate();
                ListViewItem item = null;
                SuppressItemSelectedEvent = true;

                foreach (object chunk in chunks)
                {
                    string value = chunk.ToString();
                    byte[] data = HMessage.Encode(chunk);
                    string encoded = HMessage.ToString(data);
                    string typeName = chunk.GetType().Name.Replace("Int32", "Integer");

                    item = FocusAdd(typeName, value, encoded);
                    item.ToolTipText = string.Format(CHUNK_TIP, typeName, value, data.Length, encoded);
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