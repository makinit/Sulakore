using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Habbo.Headers;

namespace Sulakore.Communication
{
    /// <summary>
    /// Represents a connection handler to intercept incoming/outgoing data from a post-shuffle hotel.
    /// </summary>
    public class HConnection : IHConnection, IDisposable
    {
        private readonly object _disconnectLock;
        private static readonly string _hostsFile;
        private readonly IDictionary<ushort, TcpListener> _listeners;

        /// <summary>
        /// Occurs when the intercepted local <see cref="HNode"/> initiates the handshake with the server.
        /// </summary>
        public event EventHandler<EventArgs> Connected;
        /// <summary>
        /// Raises the <see cref="Connected"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }
        /// <summary>
        /// Occurs when either client/server have been disconnected, or when <see cref="Disconnect"/> has been called if <see cref="IsConnected"/> is true.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;
        /// <summary>
        /// Raises the <see cref="Disconnected"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }
        /// <summary>
        /// Occurs when outgoing data from the local <see cref="HNode"/> has been intercepted.
        /// </summary>
        public event EventHandler<InterceptedEventArgs> DataOutgoing;
        /// <summary>
        /// Raises the <see cref="DataOutgoing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="InterceptedEventArgs"/> that contains the event data.</param>
        /// <returns></returns>
        protected virtual void OnDataOutgoing(InterceptedEventArgs e)
        {
            DataOutgoing?.Invoke(this, e);
        }
        /// <summary>
        /// Occurs when incoming data from the remote <see cref="HNode"/> has been intercepted.
        /// </summary>
        public event EventHandler<InterceptedEventArgs> DataIncoming;
        /// <summary>
        /// Raises the <see cref="DataIncoming"/> event.
        /// </summary>
        /// <param name="e">An <see cref="InterceptedEventArgs"/> that contains the event data.</param>
        /// <returns></returns>
        protected virtual void OnDataIncoming(InterceptedEventArgs e)
        {
            DataIncoming?.Invoke(this, e);
        }

        /// <summary>
        /// Gets the port of the remote endpoint.
        /// </summary>
        public ushort Port { get; private set; }
        /// <summary>
        /// Gets the host name of the remote endpoint.
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Gets an array of strings containing the Internet Protocol(IP) addresses resolved from the Host.
        /// </summary>
        public string[] Addresses { get; private set; }

        /// <summary>
        /// Gets the <see cref="HNode"/> representing the local connection.
        /// </summary>
        public HNode Local { get; private set; }
        /// <summary>
        /// Gets the <see cref="HNode"/> representing the remote connection.
        /// </summary>
        public HNode Remote { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the <see cref="HConnection"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="HConnection"/> has established a connection with the game.
        /// </summary>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// Gets the <see cref="IList{T}"/> that contains the headers of outgoing packets to block.
        /// </summary>
        public IList<ushort> OutgoingBlocked { get; }
        /// <summary>
        /// Gets the <see cref="IList{T}"/> that contains the headers of incoming packets to block.
        /// </summary>
        public IList<ushort> IncomingBlocked { get; }
        /// <summary>
        /// Gets the <see cref="IDictionary{TKey, TValue}"/> that contains the replacement data for an outgoing packet determined by the header.
        /// </summary>
        public IDictionary<ushort, byte[]> OutgoingReplaced { get; }
        /// <summary>
        /// Gets the <see cref="IDictionary{TKey, TValue}"/> that contains the replacement data for an incoming packet determined by the header.
        /// </summary>
        public IDictionary<ushort, byte[]> IncomingReplaced { get; }

        /// <summary>
        /// Gets the total amount of packets the remote <see cref="HNode"/> has been sent from local.
        /// </summary>
        public int TotalOutgoing { get; private set; }
        /// <summary>
        /// Gets the total amount of packets the local <see cref="HNode"/> received from remote.
        /// </summary>
        public int TotalIncoming { get; private set; }

        static HConnection()
        {
            _hostsFile = Environment.GetFolderPath(
                Environment.SpecialFolder.System) + "\\drivers\\etc\\hosts";

            if (!File.Exists(_hostsFile))
                File.Create(_hostsFile).Dispose();

            File.SetAttributes(_hostsFile, FileAttributes.Normal);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HConnection"/> class.
        /// </summary>
        public HConnection()
        {
            _disconnectLock = new object();
            _listeners = new Dictionary<ushort, TcpListener>();

            OutgoingBlocked = new List<ushort>();
            IncomingBlocked = new List<ushort>();
            OutgoingReplaced = new Dictionary<ushort, byte[]>();
            IncomingReplaced = new Dictionary<ushort, byte[]>();
        }

        /// <summary>
        /// Disconnects all concurrent connections.
        /// </summary>
        public void Disconnect()
        {
            if (Monitor.TryEnter(_disconnectLock))
            {
                try
                {
                    TcpListener[] listeners = _listeners.Values.ToArray();
                    foreach (TcpListener listener in listeners)
                    {
                        listener.Stop();
                    }

                    Local?.Dispose();
                    Remote?.Dispose();

                    TotalIncoming = TotalOutgoing = 0;
                    if (IsConnected)
                    {
                        IsConnected = false;
                        OnDisconnected(EventArgs.Empty);
                    }
                }
                finally
                {
                    Monitor.Exit(_disconnectLock);
                    RestoreHosts();
                }
            }
            else return;
        }
        /// <summary>
        /// Intercepts the attempted connection on the specified port(s), and establishes a connection with the host in an asynchronous operation.
        /// </summary>
        /// <param name="host">The host to establish a connection with.</param>
        /// <param name="ports">The port(s) to intercept the local connection attempt.</param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, params ushort[] ports)
        {
            Disconnect();
            Host = host.Split(':')[0];

            IPAddress[] hostAddresses = await Dns.GetHostAddressesAsync(
                Host).ConfigureAwait(false);

            IList<string> hosts = hostAddresses.Select(
                ip => ip.ToString()).Distinct().ToList();

            hosts.Add(Host);
            Addresses = hosts.ToArray();
            WriteHosts(Addresses);

            await InterceptClientAsync(ports)
                .ConfigureAwait(false);
        }

        private async Task InterceptClientAsync(ushort[] ports)
        {
            foreach (ushort port in ports)
            {
                if (!_listeners.ContainsKey(port))
                {
                    _listeners.Add(port,
                        new TcpListener(IPAddress.Any, port));
                }

            }
            var interceptionTasks = new List<Task>(_listeners.Count);
            foreach (TcpListener listener in _listeners.Values)
                interceptionTasks.Add(InterceptClientAsync(listener));

            Task completedInterception =
                await Task.WhenAny(interceptionTasks);

            foreach (TcpListener listener in _listeners.Values)
                listener.Stop();
        }
        private async Task InterceptClientAsync(TcpListener listener)
        {
            var port = (ushort)(
                (IPEndPoint)listener.LocalEndpoint).Port;

            try
            {
                listener.Start();
                await Task.Yield();

                while (!IsConnected)
                {
                    Socket localSocket = await listener.AcceptSocketAsync()
                        .ConfigureAwait(false);

                    Task interceptDataTask =
                        InterceptClientDataAsync(localSocket, port);
                }
            }
            catch (ObjectDisposedException) { /* Listener stopped. */ }
            finally { listener.Stop(); }
        }
        private async Task InterceptClientDataAsync(Socket client, ushort port)
        {
            var local = new HNode(client);
            var remote = await HNode.ConnectAsync(Addresses[0], port)
                .ConfigureAwait(false);

            try
            {
                var buffer = new byte[1024];
                int length = await local.ReceiveAsync(
                    buffer, 0, 6).ConfigureAwait(false);

                if (length < 1)
                {
                    local.Dispose();
                    local = new HNode(await _listeners[port]
                        .AcceptSocketAsync().ConfigureAwait(false));

                    length = await local.ReceiveAsync(
                        buffer, 0, 6).ConfigureAwait(false);
                }

                if (buffer[0] == 64) throw new Exception("Base64/VL64 protocol not supported.");
                if (BigEndian.ToUInt16(buffer, 4) == Outgoing.CLIENT_CONNECT)
                {
                    IsConnected = true;

                    Port = port;
                    Local = local;
                    Remote = remote;

                    byte[] packet = new byte[BigEndian.ToInt32(buffer, 0) + 4];
                    Buffer.BlockCopy(buffer, 0, packet, 0, 6);

                    length = await Local.ReceiveAsync(packet, 6, packet.Length - 6)
                        .ConfigureAwait(false);

                    OnConnected(EventArgs.Empty);
                    HandleOutgoing(packet, ++TotalOutgoing);

                    Task readInTask = ReadIncomingAsync();
                }
                else
                {
                    length += await local.ReceiveAsync(buffer, length, buffer.Length - length)
                        .ConfigureAwait(false);

                    await remote.SendAsync(buffer, 0, length)
                        .ConfigureAwait(false);

                    length = await remote.ReceiveAsync(buffer, 0, buffer.Length)
                        .ConfigureAwait(false);

                    await local.SendAsync(buffer, 0, length)
                        .ConfigureAwait(false);

                    // Send empty data to local socket.
                    await local.SendAsync(buffer, 0, 0)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                if (Local != local)
                    local.Dispose();

                if (Remote != remote)
                    remote.Dispose();
            }
        }

        /// <summary>
        /// Sends data to the remote <see cref="HNode"/> in an asynchronous operation.
        /// </summary>
        /// <param name="data">The data to send to the node.</param>
        /// <returns></returns>
        public Task<int> SendToServerAsync(byte[] data)
        {
            return Remote.SendAsync(data);
        }
        /// <summary>
        /// Sends data to the remote <see cref="HNode"/> using the specified header, and chunks in an asynchronous operation.
        /// </summary>
        /// <param name="header">The header to be used for the construction of the packet.</param>
        /// <param name="chunks">The chunks/values that the packet will carry.</param>
        /// <returns></returns>
        public Task<int> SendToServerAsync(ushort header, params object[] chunks)
        {
            return Remote.SendAsync(HMessage.Construct(header, chunks));
        }

        /// <summary>
        /// Sends data to the local <see cref="HNode"/> in an asynchronous operation.
        /// </summary>
        /// <param name="data">The data to send to the node.</param>
        /// <returns></returns>
        public Task<int> SendToClientAsync(byte[] data)
        {
            return Local.SendAsync(data);
        }
        /// <summary>
        /// Sends data to the local <see cref="HNode"/> using the specified header, and chunks in an asynchronous operation.
        /// </summary>
        /// <param name="header">The header to be used for the construction of the packet.</param>
        /// <param name="chunks">The chunks/values that the packet will carry.</param>
        /// <returns></returns>
        public Task<int> SendToClientAsync(ushort header, params object[] chunks)
        {
            return Local.SendAsync(HMessage.Construct(header, chunks));
        }

        private async Task ReadOutgoingAsync()
        {
            byte[] packet = await Local.ReceiveWireMessageAsync()
                .ConfigureAwait(false);

            if (packet == null) Disconnect();
            else HandleOutgoing(packet, ++TotalOutgoing);
        }
        private void HandleOutgoing(byte[] data, int count)
        {
            var args = new InterceptedEventArgs(ReadOutgoingAsync,
                count, new HMessage(data, HDestination.Server));

            if (OutgoingReplaced.ContainsKey(args.Packet.Header))
            {
                ushort header = args.Packet.Header;
                args.Replacement = new HMessage(OutgoingReplaced[header], HDestination.Server);
            }

            args.IsBlocked = OutgoingBlocked.Contains(args.Replacement.Header);
            OnDataOutgoing(args);

            if (!args.IsBlocked)
                SendToServerAsync(args.Replacement.ToBytes()).Wait();

            if (!args.WasContinued)
            {
                Task readOutTask = ReadOutgoingAsync();
            }
        }

        private async Task ReadIncomingAsync()
        {
            byte[] packet = await Remote.ReceiveWireMessageAsync()
                .ConfigureAwait(false);

            if (packet == null) Disconnect();
            else HandleIncoming(packet, ++TotalIncoming);
        }
        private void HandleIncoming(byte[] data, int count)
        {
            var args = new InterceptedEventArgs(ReadIncomingAsync,
                count, new HMessage(data, HDestination.Client));

            if (IncomingReplaced.ContainsKey(args.Packet.Header))
            {
                ushort header = args.Packet.Header;
                args.Replacement = new HMessage(IncomingReplaced[header], HDestination.Client);
            }

            args.IsBlocked = IncomingBlocked.Contains(args.Replacement.Header);
            OnDataIncoming(args);

            if (!args.IsBlocked)
                SendToClientAsync(args.Replacement.ToBytes()).Wait();

            if (!args.WasContinued)
            {
                Task readInTask = ReadIncomingAsync();
            }
        }

        /// <summary>
        /// Removes all lines from the hosts file containing: #Sulakore
        /// </summary>
        public static void RestoreHosts()
        {
            IEnumerable<string> lines = File.ReadAllLines(_hostsFile)
                .Where(line => !line.EndsWith("#Sulakore") &&
                !string.IsNullOrWhiteSpace(line));

            File.WriteAllLines(_hostsFile, lines);
        }
        public static void WriteHosts(params string[] hosts)
        {
            string hostsContent = string.Empty;
            foreach (string host in hosts)
            {
                string mapping = $"127.0.0.1\t\t{host}\t\t#Sulakore\r\n";
                if (hostsContent.Contains(mapping)) continue;
                hostsContent += mapping;
            }
            File.AppendAllText(_hostsFile, hostsContent);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="HConnection"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// Releases all resources used by the <see cref="HConnection"/>.
        /// </summary>
        /// <param name="disposing">The value that determines whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                SKore.Unsubscribe(ref Connected);
                SKore.Unsubscribe(ref Disconnected);
                SKore.Unsubscribe(ref DataIncoming);
                SKore.Unsubscribe(ref DataOutgoing);
                Disconnect();
            }
            IsDisposed = true;
        }
    }
}