using System;
using System.Threading.Tasks;

namespace Sulakore.Communication
{
    public interface IHConnection
    {
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<DataInterceptedEventArgs> DataOutgoing;
        event EventHandler<DataInterceptedEventArgs> DataIncoming;

        ushort Port { get; }
        string Host { get; }
        string Address { get; }

        int TotalOutgoing { get; }
        int TotalIncoming { get; }
        
        Task<int> SendToServerAsync(byte[] data);
        Task<int> SendToServerAsync(ushort header, params object[] chunks);

        Task<int> SendToClientAsync(byte[] data);
        Task<int> SendToClientAsync(ushort header, params object[] chunks);
    }
}