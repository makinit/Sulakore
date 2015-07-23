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
using System.Reflection;
using System.Windows.Forms;

using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionForm : Form
    {
        /// <summary>
        /// Gets a value that determines whether the <see cref="ExtensionForm"/> is running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets the name of <see cref="ExtensionForm"/> creator.
        /// </summary>
        public string Creator { get; }
        /// <summary>
        /// Gets the name of the <see cref="ExtensionForm"/>.
        /// </summary>
        public string Identifier { get; }
        /// <summary>
        /// Gets the description of the <see cref="ExtensionForm"/>.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the file path of the <see cref="ExtensionForm"/> assembly.
        /// </summary>
        public string FileLocation { get; }

        /// <summary>
        /// Gets the assembly's <see cref="System.Version"/> of the <see cref="ExtensionForm"/>.
        /// </summary>
        public Version Version { get; }
        /// <summary>
        /// Gets the <see cref="HTriggers"/> that handles the in-game callbacks/events.
        /// </summary>
        public HTriggers Triggers { get; protected set; }

        public HHotel Hotel { get; }
        public IHConnection Connection { get; }

        public ExtensionForm()
        {
            var extensionAssembly = Assembly.GetCallingAssembly();
            ExtensionInfo extensionInfo = Contractor.GetExtensionInfo(extensionAssembly);

            if (extensionInfo != null)
            {
                Creator = extensionInfo.Creator;
                Identifier = extensionInfo.Identifier;
                Description = extensionInfo.Description;
                FileLocation = extensionInfo.FileLocation;

                Triggers = extensionInfo.Triggers;
                Version = extensionInfo.Version;

                Hotel = extensionInfo.Hotel;
                Connection = extensionInfo.Connection;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            IsRunning = true;
            base.OnShown(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                IsRunning = false;

                if (Triggers != null)
                    Triggers.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}