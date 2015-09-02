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
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

using Sulakore.Habbo.Web;
using Sulakore.Communication;
using System.Security.Cryptography;
using System.IO;

namespace Sulakore.Extensions
{
    public class ExtensionForm : Form
    {
        /// <summary>
        /// Gets the MD5 hash of the file that contains the assembly data of this extension.
        /// </summary>
        [Browsable(false)]
        public string Hash { get; }
        /// <summary>
        /// Gets a value that determines whether this extension has been installed to a <see cref="Contractor"/>.
        /// </summary>
        [Browsable(false)]
        public bool IsInstalled { get; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="ExtensionForm"/> is running.
        /// </summary>
        [Browsable(false)]
        public bool IsRunning { get; internal set; }
        /// <summary>
        /// Gets the name of <see cref="ExtensionForm"/> creator.
        /// </summary>
        [Browsable(false)]
        public string Creator { get; }
        /// <summary>
        /// Gets the name of the <see cref="ExtensionForm"/>.
        /// </summary>
        [Browsable(false)]
        public string Identifier { get; }
        /// <summary>
        /// Gets the description of the <see cref="ExtensionForm"/>.
        /// </summary>
        [Browsable(false)]
        public string Description { get; }
        /// <summary>
        /// Gets the file path of the <see cref="ExtensionForm"/> assembly.
        /// </summary>
        [Browsable(false)]
        public string FileLocation { get; }
        /// <summary>
        /// Gets the <see cref="HGameData"/> of the <see cref="ExtensionForm"/> assembly.
        /// </summary>
        [Browsable(false)]
        public HGameData GameData { get; }
        /// <summary>
        /// Gets the assembly's <see cref="System.Version"/> of the <see cref="ExtensionForm"/>.
        /// </summary>
        [Browsable(false)]
        public Version Version { get; }
        /// <summary>
        /// Gets the <see cref="HTriggers"/> that handles the in-game callbacks/events.
        /// </summary>
        [Browsable(false)]
        public HTriggers Triggers { get; protected set; }
        /// <summary>
        /// Gets the <see cref="HHotel"/> object that represents the hotel currently connected to.
        /// </summary>
        [Browsable(false)]
        public HHotel Hotel { get; }
        /// <summary>
        /// Gets the <see cref="IHConnection"/> instance used for blocking, replacing, sending, and intercepting data.
        /// </summary>
        [Browsable(false)]
        public IHConnection Connection { get; }

        public ExtensionForm()
        {
            Triggers = new HTriggers(false);
            var extensionAssembly = Assembly.GetCallingAssembly();

            ExtensionInfo extensionInfo =
                Contractor.GetExtensionInfo(extensionAssembly);

            if (IsInstalled = (extensionInfo != null))
            {
                Hash = extensionInfo.Hash;
                Hotel = extensionInfo.Hotel;
                GameData = extensionInfo.GameData;
                Connection = extensionInfo.Connection;
                FileLocation = extensionInfo.FileLocation;

                var fileInfo = FileVersionInfo.GetVersionInfo(FileLocation);
                Version = new Version(fileInfo.ProductVersion);
                Identifier = fileInfo.FileDescription;
                Description = fileInfo.Comments;
                Creator = fileInfo.CompanyName;
            }
        }

        protected virtual void OnDisposed()
        { }
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
                Triggers?.Dispose();
                OnDisposed();
            }
            base.Dispose(disposing);
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            IsRunning = false;
            base.OnFormClosed(e);
        }
    }
}