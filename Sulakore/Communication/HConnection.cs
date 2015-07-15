﻿/* Copyright

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
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Habbo.Headers;
using Sulakore.Protocol.Encoders;

namespace Sulakore.Communication
{
    /// <summary>
    /// Represents a connection handler to intercept incoming/outgoing data from a post-shuffle hotel.
    /// </summary>
    public class HConnection : IHConnection, IDisposable
    {
        private TcpListener _listener;
        private readonly object _disconnectLock;
        private static readonly string _hostsFile;

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
            EventHandler<EventArgs> handler = Connected;
            if (handler != null) handler(this, e);
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
            EventHandler<EventArgs> handler = Disconnected;
            if (handler != null) handler(this, e);
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
            EventHandler<InterceptedEventArgs> handler = DataOutgoing;
            if (handler != null) handler(this, e);
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
            EventHandler<InterceptedEventArgs> handler = DataIncoming;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets the port of the remote endpoint.
        /// </summary>
        public int Port { get; private set; }
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
        public IList<ushort> OutgoingBlocked { get; private set; }
        /// <summary>
        /// Gets the <see cref="IList{T}"/> that contains the headers of incoming packets to block.
        /// </summary>
        public IList<ushort> IncomingBlocked { get; private set; }
        /// <summary>
        /// Gets the <see cref="IDictionary{TKey, TValue}"/> that contains the replacement data for an outgoing packet determined by the header.
        /// </summary>
        public IDictionary<ushort, byte[]> OutgoingReplaced { get; private set; }
        /// <summary>
        /// Gets the <see cref="IDictionary{TKey, TValue}"/> that contains the replacement data for an incoming packet determined by the header.
        /// </summary>
        public IDictionary<ushort, byte[]> IncomingReplaced { get; private set; }

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
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HConnection"/> class.
        /// </summary>
        public HConnection()
        {
            _disconnectLock = new object();

            OutgoingBlocked = new List<ushort>();
            IncomingBlocked = new List<ushort>();
            OutgoingReplaced = new Dictionary<ushort, byte[]>();
            IncomingReplaced = new Dictionary<ushort, byte[]>();

            if (!File.Exists(_hostsFile))
                File.Create(_hostsFile).Dispose();

            File.SetAttributes(_hostsFile, FileAttributes.Normal);
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
                    if (_listener != null)
                        _listener.Stop();

                    if (Local != null)
                        Local.Dispose();

                    if (Remote != null)
                        Remote.Dispose();

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
        /// Intercepts the attempted connection on the specified port, and establishes a connection with the host in an asynchronous operation.
        /// </summary>
        /// <param name="host">The host to establish a connection with.</param>
        /// <param name="port">The port to intercept the local connection attempt.</param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, int port)
        {
            Port = port;
            Host = host;
            Disconnect();

            var hostsFileBuilder = new StringBuilder(string.Format(
                "127.0.0.1\t\t{0}\t\t#Sulakore\r\n", host));

            Addresses = (await Dns.GetHostAddressesAsync(host)
                .ConfigureAwait(false))
                .Select(ip => ip.ToString()).ToArray();

            foreach (string address in Addresses)
            {
                hostsFileBuilder.AppendLine(string.Format(
                    "127.0.0.1\t\t{0}\t\t#Sulakore", address));
            }

            File.AppendAllText(_hostsFile, hostsFileBuilder.ToString());
            await InterceptClientAsync().ConfigureAwait(false);
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

        private async Task InterceptClientAsync()
        {
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();

            try
            {
                while (!IsConnected)
                {
                    var localSocket = await _listener.AcceptSocketAsync()
                        .ConfigureAwait(false);

                    Local = new HNode(localSocket);
                    Remote = await HNode.ConnectAsync(Addresses[0], Port)
                        .ConfigureAwait(false);

                    byte[] outBuffer = new byte[6];
                    int length = await Local.ReceiveAsync(outBuffer,
                        0, outBuffer.Length).ConfigureAwait(false);

                    if (length == 0) return;
                    if (BigEndian.ToUI16(outBuffer, 4) == Outgoing.CLIENT_CONNECT)
                    {
                        byte[] packet = new byte[BigEndian.ToSI32(outBuffer) + 4];
                        Buffer.BlockCopy(outBuffer, 0, packet, 0, 6);

                        length = await Local.ReceiveAsync(packet,
                            6, packet.Length - 6).ConfigureAwait(false);

                        if (length == 0) return;

                        IsConnected = true;
                        OnConnected(EventArgs.Empty);

                        HandleOutgoing(packet, ++TotalOutgoing);
                        ReadIncomingAsync();
                    }
                    else
                    {
                        byte[] newOutBuffer = new byte[1000];
                        Buffer.BlockCopy(outBuffer, 0, newOutBuffer, 0, 6);

                        length = await Local.ReceiveAsync(newOutBuffer, 6, newOutBuffer.Length - 6)
                            .ConfigureAwait(false);

                        if (length == 0) return;
                        await Remote.SendAsync(newOutBuffer, 0, length + 6)
                            .ConfigureAwait(false);

                        length = await Remote.ReceiveAsync(newOutBuffer, 0, newOutBuffer.Length)
                            .ConfigureAwait(false);

                        if (length == 0) return;
                        await Local.SendAsync(newOutBuffer, 0, length)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (ObjectDisposedException) { /* Listener stopped. */ }
            finally
            {
                _listener.Stop();
                _listener = null;
            }
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
                count, data, HDestination.Server);

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
                ReadOutgoingAsync();
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
                count, data, HDestination.Client);

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
                ReadIncomingAsync();
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