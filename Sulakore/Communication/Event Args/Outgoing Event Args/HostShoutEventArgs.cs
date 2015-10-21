/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostShoutEventArgs : InterceptedEventArgs
    {
        public HTheme Theme { get; }
        public string Message { get; }

        public HostShoutEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Message = packet.ReadString();

            // TODO: Find the chunks before OwnerId and read them.
            Theme = (HTheme)packet.ReadInteger(packet.Length - 6);
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Message)}: {Message}, {nameof(Theme)}: {Theme}";
    }
}