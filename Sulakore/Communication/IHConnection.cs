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

        IList<ushort> OutgoingBlocked { get; }
        IList<ushort> IncomingBlocked { get; }
        IDictionary<ushort, byte[]> OutgoingReplaced { get; }
        IDictionary<ushort, byte[]> IncomingReplaced { get; }

        int TotalOutgoing { get; }
        int TotalIncoming { get; }

        int GameHostPort { get; }
        string GameHostName { get; }

        Task<int> SendToServerAsync(byte[] data);
        Task<int> SendToServerAsync(ushort header, params object[] chunks);

        Task<int> SendToClientAsync(byte[] data);
        Task<int> SendToClientAsync(ushort header, params object[] chunks);
    }
}