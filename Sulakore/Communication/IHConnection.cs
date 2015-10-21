/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sulakore.Communication
{
    public interface IHConnection
    {
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<InterceptedEventArgs> DataOutgoing;
        event EventHandler<InterceptedEventArgs> DataIncoming;

        ushort Port { get; }
        string Host { get; }
        string[] Addresses { get; }

        IList<ushort> OutgoingBlocked { get; }
        IList<ushort> IncomingBlocked { get; }
        IDictionary<ushort, byte[]> OutgoingReplaced { get; }
        IDictionary<ushort, byte[]> IncomingReplaced { get; }

        int TotalOutgoing { get; }
        int TotalIncoming { get; }

        Task<int> SendToServerAsync(byte[] data);
        Task<int> SendToServerAsync(ushort header, params object[] chunks);

        Task<int> SendToClientAsync(byte[] data);
        Task<int> SendToClientAsync(ushort header, params object[] chunks);
    }
}