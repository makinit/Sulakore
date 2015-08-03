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

using Sulakore.Extensions;
using Sulakore.Communication;

namespace Sulakore.Components
{
    [DesignerCategory("Code")]
    public class SKoreExtensionView : SKoreListView
    {
        private readonly IDictionary<ExtensionForm, ListViewItem> _itemByExtensionForm;
        private readonly IDictionary<ListViewItem, ExtensionForm> _extensionFormByItem;

        public bool AutoOpen { get; set; }
        public Contractor Contractor { get; private set; }

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

                extension = Contractor.ReininitializeExtension(extension);
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

            Contractor.Uninstall(extension);
        }
        public ExtensionForm GetSelected()
        {
            ListViewItem item = GetSelectedItem();
            if (item == null) return null;

            return _extensionFormByItem[item];
        }
        public ExtensionForm Install(string path)
        {
            return Contractor.Install(path);
        }

        public Contractor InitializeContractor(Contractor contractor)
        {
            Contractor = contractor;
            Contractor.ExtensionAction += Contractor_ExtensionAction;

            Contractor.LoadInstalledExtensions();
            return Contractor;
        }
        public Contractor InitializeContractor(HConnection connection)
        {
            if (Contractor != null)
                throw new Exception("The contractor has already been initialized.");

            return InitializeContractor(new Contractor(connection));
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

                    Items.Remove(extensionItem);
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