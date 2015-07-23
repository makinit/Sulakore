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
using System.Net.Sockets;
using System.Threading.Tasks;

using Sulakore.Protocol.Encoders;
using Sulakore.Protocol.Encryption;
using System.Net;

namespace Sulakore.Communication
{
    /// <summary>
    /// Represents a wrapper for the <see cref="Socket"/> that allows asynchronous operations using <see cref="Task"/>.
    /// </summary>
    public class HNode : IDisposable
    {
        private int _total;

        /// <summary>
        /// Gets the underlying <see cref="Socket"/>.
        /// </summary>
        public Socket Client { get; }
        /// <summary>
        /// Gets or sets the <see cref="Rc4"/> for encrypting the data being sent.
        /// </summary>
        public Rc4 Encrypter { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="Rc4"/> for decrypting the data being received.
        /// </summary>
        public Rc4 Decrypter { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="HKeyExchange"/> used for handling RSA encryption/decryption when exchanging the Diffie–Hellman keys.
        /// </summary>
        public HKeyExchange Exchange { get; set; }
        /// <summary>
        /// Gets the <see cref="NetworkStream"/> used to send and receive data.
        /// </summary>
        public NetworkStream SocketStream { get; }
        /// <summary>
        /// Gets or sets the value that determines whether the <see cref="HNode"/> has already been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets a value that determines whether receiving data needs to be decrypted.
        /// </summary>
        public bool IsDecryptionRequired { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HNode"/> class.
        /// </summary>
        /// <param name="client"></param>
        public HNode(Socket client)
        {
            if (client == null)
                throw new NullReferenceException(nameof(client));

            Client = client;
            SocketStream = new NetworkStream(Client);
        }

        /// <summary>
        /// Sends data to a connected <see cref="HNode"/> in an asynchronous operation.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to be sent.</param>
        /// <returns></returns>
        public Task<int> SendAsync(byte[] buffer)
        {
            buffer = Encrypter?.SafeParse(buffer) ?? buffer;
            return SendAsync(buffer, 0, buffer.Length);
        }
        /// <summary>
        /// Sends data to a connected <see cref="HNode"/> in an asynchronous operation.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to send.</param>
        /// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <returns></returns>
        public async Task<int> SendAsync(byte[] buffer, int offset, int size)
        {
            try
            {
                IAsyncResult result = Client.BeginSend(buffer, offset,
                    size, SocketFlags.None, null, null);

                int length = await Task.Factory.FromAsync(
                    result, Client.EndSend).ConfigureAwait(false);

                return length;
            }
            catch (ObjectDisposedException) { return 0; }
        }

        /// <summary>
        /// Receives an array of type <see cref="byte"/> that is of a specific length determined by the four bytes at the beginning in an asynchronous operation.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> ReceiveWireMessageAsync()
        {
            byte[] lengthBlock = new byte[4];
            int length = await ReceiveAsync(lengthBlock, 0, 4)
                .ConfigureAwait(false);

            if (length == 0) return null;
            if (++_total == 3 || _total == 4)
            {
                length = BigEndian.ToSI32(lengthBlock);
                await Task.Delay(250).ConfigureAwait(false);

                if (Client.Available >= length && length >= 2)
                {
                    IsDecryptionRequired = false;
                    Decrypter = null;
                }
                else IsDecryptionRequired = true;
            }

            Decrypter?.Parse(lengthBlock);
            length = BigEndian.ToSI32(lengthBlock);

            int bytesRead = 0;
            int totalBytesRead = 0;
            byte[] body = new byte[length];
            while (totalBytesRead != body.Length)
            {
                byte[] block = new byte[body.Length - totalBytesRead];
                bytesRead = await ReceiveAsync(block, 0, block.Length)
                    .ConfigureAwait(false);

                if (bytesRead == 0) return null;
                Buffer.BlockCopy(block, 0, body, totalBytesRead, bytesRead);
                totalBytesRead += bytesRead;
            }
            Decrypter?.Parse(body);

            byte[] packet = new byte[4 + body.Length];
            Buffer.BlockCopy(lengthBlock, 0, packet, 0, 4);
            Buffer.BlockCopy(body, 0, packet, 4, body.Length);

            return packet;
        }
        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="HNode"/> into the specified offset position of the receive buffer in an asynchronous operation.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <returns></returns>
        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int size)
        {
            try
            {
                IAsyncResult result = Client.BeginReceive(buffer, offset,
                     size, SocketFlags.None, null, null);

                int length = await Task.Factory.FromAsync(
                    result, Client.EndReceive).ConfigureAwait(false);

                return length;
            }
            catch (ObjectDisposedException) { return 0; }
        }

        /// <summary>
        /// Returns a <see cref="HNode"/> connected with the specified host/port in an asynchronous operation.
        /// </summary>
        /// <param name="host">The host to establish the connection with.</param>
        /// <param name="port">The port to establish the connection with.</param>
        /// <returns></returns>
        public static async Task<HNode> ConnectAsync(string host, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult result = socket.BeginConnect(
                host, port, null, null);

            await Task.Factory.FromAsync(result,
                socket.EndConnect).ConfigureAwait(false);

            return new HNode(socket);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="HNode"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// Releases all resources used by the <see cref="HNode"/>.
        /// </summary>
        /// <param name="disposing">The value that determines whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                Exchange?.Dispose();
                SocketStream.Dispose();
                Client.Shutdown(SocketShutdown.Both);
                Client.Close();

                Encrypter = null;
                Decrypter = null;
            }
            IsDisposed = true;
        }
    }
}