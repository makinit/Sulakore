using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionBridge : IHConnection
    {
        private readonly ExtensionForm _extension;
        private readonly HNode _externalContractor;

        public event EventHandler<EventArgs> Connected;
        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        public event EventHandler<EventArgs> Disconnected;
        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        public event EventHandler<InterceptedEventArgs> DataOutgoing;
        protected virtual void OnDataOutgoing(InterceptedEventArgs e)
        {
            DataOutgoing?.Invoke(this, e);
        }

        public event EventHandler<InterceptedEventArgs> DataIncoming;
        protected virtual void OnDataIncoming(InterceptedEventArgs e)
        {
            DataIncoming?.Invoke(this, e);
        }

        public ushort Port { get; private set; }
        public string Host { get; private set; }
        public string[] Addresses { get; private set; }

        public IList<ushort> IncomingBlocked { get; }
        public IDictionary<ushort, byte[]> IncomingReplaced { get; }

        public IList<ushort> OutgoingBlocked { get; }
        public IDictionary<ushort, byte[]> OutgoingReplaced { get; }

        public int TotalIncoming { get; private set; }
        public int TotalOutgoing { get; private set; }

        public ExtensionBridge(HNode externalContractor, ExtensionForm extension)
        {
            _extension = extension;
            _externalContractor = externalContractor;

            OutgoingBlocked = new List<ushort>();
            IncomingBlocked = new List<ushort>();
            OutgoingReplaced = new Dictionary<ushort, byte[]>();
            IncomingReplaced = new Dictionary<ushort, byte[]>();

            RequestInformationAsync().Wait();
            Task readTask = ReadMessageAsync();
        }

        private async Task ReadMessageAsync()
        {
            byte[] readData = await _externalContractor
                .ReceiveWireMessageAsync().ConfigureAwait(false);

            var readMessage = new HMessage(readData);
            var destination = (HDestination)readMessage.Header;
            HandleMessage(readMessage.ReadBytes(readMessage.Length - 2), destination);
        }
        public async Task RequestInformationAsync()
        {
            byte[] connectionData = await _externalContractor
                .ReceiveWireMessageAsync().ConfigureAwait(false);

            var connectionMessage = new HMessage(connectionData);
            Port = connectionMessage.ReadShort();
            Host = connectionMessage.ReadString();

            Addresses = new string[connectionMessage.ReadInteger()];
            for (int i = 0; i < Addresses.Length; i++)
                Addresses[0] = connectionMessage.ReadString();
        }
        private void HandleMessage(byte[] data, HDestination destination)
        {
            var args = new InterceptedEventArgs(ReadMessageAsync,
                0, new HMessage(data, destination));

            Task readTask = ReadMessageAsync(); // Keep reading.
            if (destination == HDestination.Server)
            {
                _extension.Triggers.HandleOutgoing(args);
                OnDataOutgoing(args);
            }
            else
            {
                _extension.Triggers.HandleIncoming(args);
                OnDataIncoming(args);
            }
        }

        public Task<int> SendToClientAsync(byte[] data)
        {
            var inMessage = new HMessage(0, data);
            return _externalContractor.SendAsync(inMessage.ToBytes());
        }
        public Task<int> SendToClientAsync(ushort header, params object[] chunks)
        {
            var inMessage = new HMessage(0, HMessage.Construct(header, chunks));
            return _externalContractor.SendAsync(inMessage.ToBytes());
        }

        public Task<int> SendToServerAsync(byte[] data)
        {
            var inMessage = new HMessage(1, data);
            return _externalContractor.SendAsync(inMessage.ToBytes());
        }
        public Task<int> SendToServerAsync(ushort header, params object[] chunks)
        {
            var inMessage = new HMessage(1, HMessage.Construct(header, chunks));
            return _externalContractor.SendAsync(inMessage.ToBytes());
        }
    }
}