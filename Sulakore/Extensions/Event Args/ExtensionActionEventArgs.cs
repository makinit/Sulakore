using System;

namespace Sulakore.Extensions
{
    public class ExtensionActionEventArgs : EventArgs
    {
        public ExtensionForm Extension { get; }
        public ExtensionActionType Action { get; }

        public ExtensionActionEventArgs(
            ExtensionForm extension, ExtensionActionType action)
        {
            Action = action;
            Extension = extension;
        }
    }
}