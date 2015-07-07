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
        public string Creator { get; private set; }
        /// <summary>
        /// Gets the name of the <see cref="ExtensionForm"/>.
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Gets the description of the <see cref="ExtensionForm"/>.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Gets the file path of the <see cref="ExtensionForm"/> assembly.
        /// </summary>
        public string FileLocation { get; private set; }

        /// <summary>
        /// Gets the assembly's <see cref="System.Version"/> of the <see cref="ExtensionForm"/>.
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// Gets the <see cref="HTriggers"/> that handles the in-game callbacks/events.
        /// </summary>
        public HTriggers Triggers { get; protected set; }

        public HHotel Hotel { get; private set; }
        public IHConnection Connection { get; private set; }

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