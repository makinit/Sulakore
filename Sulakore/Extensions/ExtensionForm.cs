﻿using System;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

using Sulakore.Habbo.Web;
using Sulakore.Communication;

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