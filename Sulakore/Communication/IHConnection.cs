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

        int Port { get; }
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