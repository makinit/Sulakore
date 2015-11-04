using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Communication;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Email: {Email}, Hotel: {Hotel}")]
    public class HSession : IDisposable
    {
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

        public event EventHandler<InterceptedEventArgs> DataIncoming;
        protected virtual void OnDataIncoming(InterceptedEventArgs e)
        {
            DataIncoming?.Invoke(this, e);
        }

        private readonly Uri _hotelUri;
        private readonly object _disconnectLock;

        public HHotel Hotel { get; }
        public string Email { get; }
        public string Password { get; }
        public HTriggers Triggers { get; }
        public HRequest Requester { get; }

        public string SsoTicket { get; private set; }
        public int TotalIncoming { get; private set; }

        public HUser User { get; private set; }
        public HNode Remote { get; private set; }
        public HGameData GameData { get; private set; }

        public bool IsDisposed { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsAuthenticated { get; private set; }

        private bool _isReading;
        public bool IsReading
        {
            get { return _isReading; }
            set
            {
                if (_isReading == value)
                    return;

                if (_isReading = value)
                {
                    Task readInTask = ReadIncomingAsync();
                }
            }
        }

        static HSession()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }
        public HSession(string email, string password, HHotel hotel)
        {
            Email = email;
            Hotel = hotel;
            Password = password;

            Requester = new HRequest();
            Triggers = new HTriggers(false);

            _disconnectLock = new object();
            _hotelUri = new Uri(Hotel.ToUrl(true));
        }

        public void Disconnect()
        {
            if (IsConnected && Monitor.TryEnter(_disconnectLock))
            {
                try
                {
                    IsReading = false;
                    Remote.Dispose();

                    IsConnected = false;
                    OnDisconnected(EventArgs.Empty);

                    TotalIncoming = 0;
                }
                finally { Monitor.Exit(_disconnectLock); }
            }
        }
        public async Task ConnectAsync()
        {
            if (GameData == null)
            {
                GameData = await GetGameDataAsync()
                    .ConfigureAwait(false);
            }

            int port = int.Parse(GameData.Port.Split(',')[0]);
            string address = (await Dns.GetHostAddressesAsync(
                GameData.Host).ConfigureAwait(false))[0].ToString();

            Remote = await HNode.ConnectAsync(address,
                port).ConfigureAwait(false);

            await SendToServerAsync(Encoding.UTF8.GetBytes(
                "<policy-file-request/>\0")).ConfigureAwait(false);

            Remote.Dispose();
            Remote = await HNode.ConnectAsync(Dns.GetHostAddresses(GameData.Host)[0].ToString(),
                int.Parse(GameData.Port.Split(',')[0])).ConfigureAwait(false);

            IsConnected = true;
            OnConnected(EventArgs.Empty);

            IsReading = true;
        }
        public async Task<bool> LoginAsync()
        {
            try
            {
                IsAuthenticated = false;
                byte[] postData = Encoding.UTF8.GetBytes(
                    $"{{\"email\":\"{Email}\",\"password\":\"{Password}\"}}");

                var request = (HttpWebRequest)WebRequest.Create(
                    _hotelUri.OriginalString + "/api/public/authentication/login");

                request.ContentType = "application/json;charset=UTF-8";
                string body = await Requester.DownloadStringAsync(
                    request, postData).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(body))
                {
                    IsAuthenticated = true;
                    User = HUser.Create(body);
                }
            }
            catch { IsAuthenticated = false; }
            return IsAuthenticated;
        }
        public async Task<HGameData> GetGameDataAsync()
        {
            string body = await Requester.DownloadStringAsync(
                _hotelUri.OriginalString + "/api/client/clienturl").ConfigureAwait(false);

            body = body.GetChild("{\"clienturl\":\"", '"');
            SsoTicket = body.Split('/').Last();

            body = await Requester.DownloadStringAsync(body)
                .ConfigureAwait(false);

            return (GameData = new HGameData(body));
        }

        public Task<int> SendToServerAsync(byte[] data)
        {
            return Remote.SendAsync(data);
        }
        public Task<int> SendToServerAsync(ushort header, params object[] chunks)
        {
            return Remote.SendAsync(HMessage.Construct(header, chunks));
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

            Triggers.HandleIncoming(args);
            OnDataIncoming(args);

            if (!args.WasContinued && IsReading)
            {
                Task readInTask = ReadIncomingAsync();
            }
        }

        public static IList<HSession> Extract(string path, char delimiter = ':')
        {
            var sessions = new List<HSession>();
            using (var streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.Contains(delimiter))
                    {
                        string[] credentials = line.Split(delimiter);
                        if (credentials.Count(x => !string.IsNullOrEmpty(x)) != 3) break;
                        sessions.Add(new HSession(credentials[0], credentials[1], SKore.ToHotel(credentials[2])));
                        continue;
                    }
                    if (line.Contains('@') && !streamReader.EndOfStream)
                    {
                        string email = line;
                        string password = streamReader.ReadLine();
                        if (!streamReader.EndOfStream)
                        {
                            HHotel hotel = SKore.ToHotel((streamReader.ReadLine()).GetChild(" / "));
                            sessions.Add(new HSession(email, password, hotel));
                        }
                        else return sessions.ToArray();
                    }
                }
            }
            return sessions;
        }

        public override string ToString() =>
            $"{Email}:{Password}:{Hotel.ToDomain()}";

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                Triggers.Dispose();
                SKore.Unsubscribe(ref Connected);
                SKore.Unsubscribe(ref Disconnected);
                SKore.Unsubscribe(ref DataIncoming);
                Disconnect();
            }
            IsDisposed = true;
        }
    }
}