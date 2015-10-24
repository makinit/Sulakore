using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sulakore.Extensions;
using Sulakore.Communication;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreExtensionView : SKoreListView
    {
        private Contractor _contractor;

        private readonly IDictionary<ExtensionForm, ListViewItem> _itemByExtensionForm;
        private readonly IDictionary<ListViewItem, ExtensionForm> _extensionFormByItem;

        public bool AutoOpen { get; set; }

        public SKoreExtensionView()
        {
            _itemByExtensionForm = new Dictionary<ExtensionForm, ListViewItem>();
            _extensionFormByItem = new Dictionary<ListViewItem, ExtensionForm>();
        }

        public void OpenSelected()
        {
            ExtensionForm extension =
                _extensionFormByItem[GetSelectedItem()];

            if (extension.IsDisposed)
            {
                var item = _itemByExtensionForm[extension];
                _itemByExtensionForm.Remove(extension);

                extension = _contractor.Initialize(extension);
                _extensionFormByItem[item] = extension;
                _itemByExtensionForm.Add(extension, item);
            }

            if (!extension.IsRunning) extension.Show();
            else extension.BringToFront();
        }
        public void CloseSelected()
        {
            ExtensionForm extension =
                _extensionFormByItem[GetSelectedItem()];

            if (extension.IsRunning)
                extension.Close();
        }
        public void UninstallSelected()
        {
            ExtensionForm extension =
                _extensionFormByItem[GetSelectedItem()];

            _contractor.Uninstall(extension);
        }
        public ExtensionForm GetSelected()
        {
            ListViewItem item = GetSelectedItem();
            if (item == null) return null;

            return _extensionFormByItem[item];
        }
        public ExtensionForm Install(string path)
        {
            return _contractor.Install(path);
        }

        public Contractor InitializeContractor(Contractor contractor)
        {
            _contractor = contractor;
            _contractor.ExtensionAction += Contractor_ExtensionAction;

            _contractor.LoadExtensions();
            return _contractor;
        }
        public Contractor InitializeContractor(HConnection connection)
        {
            if (_contractor != null)
                throw new Exception("The contractor has already been initialized.");

            var contractor = new Contractor();
            contractor.Connection = connection;

            return InitializeContractor(contractor);
        }

        protected override void OnItemActivate(EventArgs e)
        {
            OpenSelected();
            base.OnItemActivate(e);
        }

        private void Contractor_ExtensionAction(object sender, ExtensionActionEventArgs e)
        {
            ListViewItem extensionItem = null;
            if (e.Action == ExtensionActionType.Installed)
            {
                extensionItem = FocusAdd(e.Extension.Identifier,
                   e.Extension.Creator, e.Extension.Description, e.Extension.Version.ToString(), "Closed");

                _itemByExtensionForm.Add(e.Extension, extensionItem);
                _extensionFormByItem.Add(extensionItem, e.Extension);
            }
            else extensionItem = _itemByExtensionForm[e.Extension];

            HandleExtensionAction(extensionItem, e.Action);
        }
        private ExtensionForm HandleExtensionAction(ListViewItem extensionItem,
            ExtensionActionType extensionAction)
        {
            ExtensionForm extension = _extensionFormByItem[extensionItem];
            switch (extensionAction)
            {
                case ExtensionActionType.Installed:
                {
                    if (AutoOpen)
                        extension.Show();
                    break;
                }

                case ExtensionActionType.Reinstalled:
                {
                    extension.BringToFront();
                    extensionItem.Selected = true;
                    break;
                }

                case ExtensionActionType.Uninstalled:
                {
                    _itemByExtensionForm.Remove(extension);
                    _extensionFormByItem.Remove(extensionItem);

                    RemoveItem(extensionItem);
                    break;
                }

                case ExtensionActionType.Opened:
                case ExtensionActionType.Closed:
                {
                    extensionItem.SubItems[4].Text =
                        extensionAction.ToString();

                    break;
                }
            }
            return extension;
        }
    }
}