using System;

namespace Sulakore.Extensions
{
    public class ExtensionActionEventArgs : EventArgs
    {
        public ExtensionForm Extension { get; private set; }
        public ExtensionActionType Action { get; private set; }

        public ExtensionActionEventArgs(
            ExtensionForm extension, ExtensionActionType action)
        {
            Action = action;
            Extension = extension;
        }
    }
}