/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

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