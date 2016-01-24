using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Extensions;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreExtensionView : SKoreListView
    {
        private readonly IDictionary<string, ListViewItem> _itemByHash;
        private readonly IDictionary<ListViewItem, ExtensionForm> _extensionByItem;

        [Browsable(false)]
        public Contractor Contractor { get; }

        public SKoreExtensionView()
        {
            Contractor = new Contractor();
            Contractor.ExtensionAction += Contractor_ExtensionAction;

            _itemByHash = new Dictionary<string, ListViewItem>();
            _extensionByItem = new Dictionary<ListViewItem, ExtensionForm>();

            Contractor.LoadExtensions();
        }

        protected override void OnItemActivate(EventArgs e)
        {
            base.OnItemActivate(e);
            OpenSelected();
        }
        private void Contractor_ExtensionAction(object sender, ExtensionActionEventArgs e)
        {
            ListViewItem item = null;
            if (_itemByHash.ContainsKey(e.Extension.Hash))
                item = _itemByHash[e.Extension.Hash];

            switch (e.Action)
            {
                case ExtensionActionType.Installed:
                {
                    item = FocusAdd(e.Extension.Identifier, e.Extension.Creator,
                        e.Extension.Description, e.Extension.Version, "Closed");

                    _itemByHash[e.Extension.Hash] = item;
                    _extensionByItem[item] = e.Extension;
                    break;
                }
                case ExtensionActionType.Reinstalled:
                {
                    e.Extension.BringToFront();
                    item.Selected = true;
                    break;
                }
                case ExtensionActionType.Uninstalled:
                {
                    _itemByHash.Remove(e.Extension.Hash);
                    _extensionByItem.Remove(item);
                    RemoveItem(item);
                    break;
                }
                case ExtensionActionType.Opened:
                case ExtensionActionType.Closed:
                {
                    item.SubItems[4].Text = e.Action.ToString();
                    break;
                }
            }
        }

        public void OpenSelected()
        {
            OpenExtension(GetSelectedExtension());
        }
        protected virtual void OpenExtension(ExtensionForm extension)
        {
            if (extension == null) return;
            if (extension.IsDisposed)
            {
                ListViewItem item = _itemByHash[extension.Hash];
                _itemByHash.Remove(extension.Hash);

                extension = Contractor.Initialize(extension);

                _extensionByItem[item] = extension;
                _itemByHash[extension.Hash] = item;
            }

            if (!extension.IsRunning) extension.Show();
            else extension.BringToFront();
        }

        public void CloseSelected()
        {
            CloseExtension(GetSelectedExtension());
        }
        protected virtual void CloseExtension(ExtensionForm extension)
        {
            if (extension != null &&
                extension.IsRunning)
            {
                extension.Close();
            }
        }

        public virtual void UninstallSelected()
        {
            UninstallExtension(GetSelectedExtension());
        }
        protected virtual void UninstallExtension(ExtensionForm extension)
        {
            if (extension != null)
                Contractor.Uninstall(extension);
        }

        public ExtensionForm Install(string path)
        {
            return Contractor.Install(path);
        }
        public ExtensionForm GetSelectedExtension()
        {
            ListViewItem item = GetSelectedItem();
            if (item == null) return null;

            return _extensionByItem[item];
        }
    }
}