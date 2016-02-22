﻿using System;
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
        /// Gets the IP address of the remote endpoint.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Gets the total amount of packets the remote <see cref="HNode"/> has been sent from local.
        /// </summary>
        public int TotalOutgoing { get; private set; }
        /// <summary>
        /// Gets the total amount of packets the local <see cref="HNode"/> received from remote.
        /// </summary>
        public int TotalIncoming { get; private set; }

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
        }

        /// <summary>
        /// Disconnects from the remote endpoint.
        /// </summary>
        public void Disconnect()
        {
            if (Monitor.TryEnter(_disconnectLock))
            {
                try
                {
                    TcpListener[] listeners = _listeners.Values.ToArray();
                    foreach (TcpListener listener in listeners) listener.Stop();

                    Local?.Dispose();
                    Local = null;

                    Remote?.Dispose();
                    Remote = null;

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

            Address = (await Dns.GetHostAddressesAsync(
                Host).ConfigureAwait(false))[0].ToString();

            WriteHosts(Host);
            await InterceptClientAsync(ports).ConfigureAwait(false);
        }

        private async Task InterceptClientAsync(ushort port)
        {
            try
            {
                TcpListener listener = _listeners[port];
                listener.Start();

                while (!IsConnected)
                {
                    Socket client = await listener
                        .AcceptSocketAsync().ConfigureAwait(false);

                    var local = new HNode(client);
                    var remote = Remote ?? (await HNode.ConnectAsync(
                        Address, port).ConfigureAwait(false));

                    await InterceptClientDataAsync(local,
                        remote, port).ConfigureAwait(false);
                }
            }
            catch { /* Swallow exceptions. */ }
        }
        private async Task InterceptClientAsync(ushort[] ports)
        {
            var interceptionTasks = new List<Task>(ports.Length);
            foreach (ushort port in ports)
            {
                if (!_listeners.ContainsKey(port))
                    _listeners.Add(port, new TcpListener(IPAddress.Any, port));

                interceptionTasks.Add(InterceptClientAsync(port));
            }

            while (interceptionTasks.Count > 0 && !IsConnected)
            {
                Task completedInterception = await Task.WhenAny(
                    interceptionTasks).ConfigureAwait(false);

                interceptionTasks.Remove(completedInterception);
            }

            foreach (TcpListener listener in _listeners.Values) listener.Stop();
            if (!IsConnected) Disconnect();
        }
        private async Task InterceptClientDataAsync(HNode local, HNode remote, ushort port)
        {
            try
            {
                byte[] buffer = await local.PeekAsync(6)
                    .ConfigureAwait(false);

                if (buffer == null || buffer.Length < 4)
                {
                    Remote = remote;
                    return;
                }
                if (BigEndian.ToUInt16(buffer, 4) == Outgoing.CLIENT_CONNECT)
                {
                    Port = port;
                    Local = local;
                    Remote = remote;

                    IsConnected = true;
                    OnConnected(EventArgs.Empty);

                    Task readOutgoingTask = ReadOutgoingAsync();
                    Task readIncomingTask = ReadIncomingAsync();
                }
                else
                {
                    buffer = await local.ReceiveAsync(1024).ConfigureAwait(false);
                    await remote.SendAsync(buffer).ConfigureAwait(false);

                    buffer = await remote.ReceiveAsync(1024).ConfigureAwait(false);
                    await local.SendAsync(buffer).ConfigureAwait(false);
                }
            }
            catch { /* Swallow all exceptions. */ }
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
            HMessage packet = await Local.ReceiveAsync()
                .ConfigureAwait(false);

            if (packet == null) Disconnect();
            else
            {
                packet.Destination = HDestination.Server;
                try { HandleOutgoing(packet, ++TotalOutgoing); }
                catch { Disconnect(); }
            }
        }
        private void HandleOutgoing(HMessage packet, int count)
        {
            var args = new InterceptedEventArgs(ReadOutgoingAsync, count, packet);
            OnDataOutgoing(args);

            if (!args.IsBlocked)
            {
                SendToServerAsync(args.Replacement.ToBytes()).Wait();
                HandleExecutions(args.GetExecutions());
            }
            if (!args.WasContinued)
            {
                Task readOutTask = ReadOutgoingAsync();
            }
        }

        private async Task ReadIncomingAsync()
        {
            HMessage packet = await Remote.ReceiveAsync()
                .ConfigureAwait(false);

            if (packet == null) Disconnect();
            else
            {
                packet.Destination = HDestination.Client;
                try { HandleIncoming(packet, ++TotalIncoming); }
                catch { Disconnect(); }
            }
        }
        private void HandleIncoming(HMessage packet, int count)
        {
            var args = new InterceptedEventArgs(ReadIncomingAsync, count, packet);
            OnDataIncoming(args);

            if (!args.IsBlocked)
            {
                SendToClientAsync(args.Replacement.ToBytes()).Wait();
                HandleExecutions(args.GetExecutions());
            }
            if (!args.WasContinued)
            {
                Task readInTask = ReadIncomingAsync();
            }
        }

        protected void HandleExecutions(HMessage[] executions)
        {
            var executeTaskList = new List<Task<int>>(executions.Length);
            for (int i = 0; i < executions.Length; i++)
            {
                byte[] executionData =
                    executions[i].ToBytes();

                HNode node = (executions[i].Destination ==
                    HDestination.Server ? Remote : Local);

                executeTaskList.Add(
                    node.SendAsync(executionData));
            }
            Task.WhenAll(executeTaskList).Wait();
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