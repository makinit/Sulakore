using System.Windows.Forms;
using System.Collections.ObjectModel;

using Sulakore.Protocol;
using System.ComponentModel;
using System.Collections.Generic;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreConstructView : SKoreListView
    {
        private const string CHUNK_TIP = "Type: {0}\nValue: {1}\nBlock Length: {2}\nEncoded: {3}";

        private readonly HMessage _packet;

        public int Length
        {
            get { return _packet.Length; }
        }
        public ushort Header
        {
            get { return _packet.Header; }
            set { _packet.Header = value; }
        }
        public IReadOnlyList<object> ChunksWritten
        {
            get { return _packet.ValuesWritten; }
        }

        public SKoreConstructView()
        {
            _packet = new HMessage(0);
        }

        public void Append(params object[] chunks)
        {
            _packet.Write(chunks);
            try
            {
                BeginUpdate();
                ListViewItem item = null;
                byte[] data = new byte[0];
                SuppressItemSelectedEvent = true;
                string typeName = string.Empty, value = string.Empty, encoded = string.Empty;
                foreach (object chunk in chunks)
                {
                    value = chunk.ToString();

                    data = HMessage.Encode(chunk);
                    encoded = HMessage.ToString(data);
                    typeName = chunk.GetType().Name.Replace("Int32", "Integer");

                    item = FocusAdd(typeName, value, encoded);
                    item.ToolTipText = string.Format(CHUNK_TIP, typeName, value, data.Length, encoded);
                }
                SuppressItemSelectedEvent = false;
                OnItemSelected(new ListViewItemSelectionChangedEventArgs(item, item.Index, true));
            }
            finally { EndUpdate(); }
        }

        public void ReplaceItem(object chunk)
        {
            ListViewItem item = SelectedItems[0];
            _packet.ReplaceWritten(item.Index, chunk);

            item.SubItems[0].Text = chunk.GetType().Name
                .Replace("Int32", "Integer");

            byte[] data = HMessage.Encode(chunk);
            item.SubItems[1].Text = chunk.ToString();
            item.SubItems[2].Text = HMessage.ToString(data);

            item.ToolTipText = string.Format(CHUNK_TIP,
                item.SubItems[0].Text, item.SubItems[1].Text, data.Length, item.SubItems[2].Text);
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

        public void ClearItems()
        {
            Items.Clear();
            _packet.ClearWritten();
        }
        public byte[] GetBytes()
        {
            return _packet.ToBytes();
        }
        public string GetString()
        {
            return _packet.ToString();
        }
    }
}