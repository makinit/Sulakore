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