using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using Sulakore;

namespace Eavesdrop
{
    public static class Eavesdropper
    {
        private static TcpListener _listener;

        private static readonly Regex _cookieSplitter;
        private static readonly byte[] _fakeOkResponse;
        private static readonly List<Task> _handleClientRequestTasks;
        private static readonly object _initiateLock, _terminateLock;

        public static event EventHandler<EavesdropperRequestEventArgs> EavesdropperRequest;
        private static void OnEavesdropperRequest(EavesdropperRequestEventArgs e)
        {
            EavesdropperRequest?.Invoke(null, e);
        }

        public static event EventHandler<EavesdropperResponseEventArgs> EavesdropperResponse;
        private static void OnEavesdropperResponse(EavesdropperResponseEventArgs e)
        {
            EavesdropperResponse?.Invoke(null, e);
        }

        public static bool IsSslSupported { get; set; }
        public static bool IsCacheDisabled { get; set; }
        public static bool IsRunning { get; private set; }

        public static int ListenPort { get; private set; }
        public static CertificateManager Certificates { get; }

        static Eavesdropper()
        {
            Certificates = new CertificateManager("Eavesdrop",
                "Eavesdrop Root Certificate Authority");

            _initiateLock = new object();
            _terminateLock = new object();
            _cookieSplitter = new Regex(@",(?! )");
            _handleClientRequestTasks = new List<Task>();
            _fakeOkResponse = Encoding.UTF8.GetBytes("HTTP/1.0 200\r\n\r\n");
        }

        public static void Terminate()
        {
            if (Monitor.TryEnter(_terminateLock))
            {
                try
                {
                    IsRunning = false;
                    _listener?.Stop();
                }
                finally
                {
                    Monitor.Exit(_terminateLock);
                    NetSettings.Terminate();
                }
            }
        }
        public static void Initiate(int port)
        {
            if (IsRunning) Terminate();
            if (Monitor.TryEnter(_initiateLock))
            {
                try
                {
                    IsRunning = true;
                    ListenPort = port;

                    string proxyServer = "http=127.0.0.1:" + port;

                    if (IsSslSupported)
                        proxyServer += ";https=127.0.0.1:" + port;

                    NetSettings.Initiate(
                        proxyServer, "<-loopback>;<local>");

                    Task.Factory.StartNew(ReadClientRequestLoop,
                        TaskCreationOptions.LongRunning);
                }
                catch
                {
                    ListenPort = 0;
                    IsRunning = false;
                    NetSettings.Terminate();
                }
                finally { Monitor.Exit(_initiateLock); }
            }
        }

        private static void ReadClientRequestLoop()
        {
            _listener = new TcpListener(
                IPAddress.Loopback, ListenPort);

            try
            {
                _listener.Start();
                while (IsRunning)
                {
                    if (_handleClientRequestTasks.Count >= 2)
                    {
                        Task.WaitAny(_handleClientRequestTasks.ToArray(), 500);
                        _handleClientRequestTasks.RemoveAll(t => t.IsCompleted);
                    }

                    try
                    {
                        if (!IsRunning) break;
                        Socket client = _listener.AcceptSocket();

                        Task readRequestTask = Task.Factory.StartNew(
                            () => HandleClientRequest(client));

                        _handleClientRequestTasks.Add(readRequestTask);
                    }
                    catch { /* Listener stopped. */ }
                }
            }
            finally
            {
                _listener.Stop();
                _handleClientRequestTasks.Clear();
            }
        }
        private static void HandleClientRequest(Socket socket)
        {
            Stream clientStream = null;
            try
            {
                byte[] postRequestPayload = null;
                clientStream = new NetworkStream(socket, true);

                byte[] requestCommandPayload =
                    ReadRequestStreamData(clientStream, ref postRequestPayload);

                if (requestCommandPayload.Length < 1) return;
                string requestCommand = Encoding.ASCII.GetString(requestCommandPayload);

                if (requestCommand.StartsWith("CONNECT"))
                {
                    string target = requestCommand.GetChild(
                        "CONNECT ", ' ').Split(':')[0];

                    socket.Send(_fakeOkResponse);
                    string host = requestCommand.GetChild("Host: ", '\r');
                    clientStream = GetSecureClientStream(host, clientStream);

                    requestCommandPayload =
                        ReadRequestStreamData(clientStream, ref postRequestPayload);

                    if (requestCommandPayload.Length < 1) return;
                    requestCommand = Encoding.ASCII.GetString(requestCommandPayload);

                    if (!(clientStream is SslStream))
                        requestCommand = "GET http://" + target + requestCommand;
                }

                HttpWebRequest request = CreateWebRequest(requestCommand);
                var requestArgs = new EavesdropperRequestEventArgs(request);
                requestArgs.Payload = postRequestPayload;

                if (IsCacheDisabled)
                    request.Headers["Cache-Control"] = "no-cache, no-store";

                OnEavesdropperRequest(requestArgs);
                if (requestArgs.Cancel) return;

                if (requestArgs.ResponsePayload != null)
                {
                    clientStream.Write(requestArgs.ResponsePayload,
                        0, requestArgs.ResponsePayload.Length);
                    return;
                }

                if (requestArgs.Payload != null && requestArgs.Payload.Length > 0)
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(requestArgs.Payload,
                            0, requestArgs.Payload.Length);
                    }
                }

                HandleClientResponse(requestArgs, clientStream);
            }
            finally { clientStream?.Dispose(); }
        }
        private static void HandleClientResponse(EavesdropperRequestEventArgs requestArgs, Stream clientStream)
        {
            WebResponse webResponse = null;
            try { webResponse = requestArgs.Request.GetResponse(); }
            catch (WebException e) { webResponse = e.Response; }

            if (webResponse == null) return;
            using (WebResponse response = webResponse)
            {
                if (IsCacheDisabled)
                    response.Headers["Cache-Control"] = "no-cache, no-store";

                byte[] responsePayload = null;
                using (var responseStream = response.GetResponseStream())
                using (var responseBufferStream = new MemoryStream())
                {
                    responseStream.CopyTo(responseBufferStream);
                    responsePayload = responseBufferStream.ToArray();
                }

                var responseArgs = new EavesdropperResponseEventArgs(response);
                responseArgs.Payload = responsePayload;

                OnEavesdropperResponse(responseArgs);
                if (!responseArgs.Cancel)
                {
                    string responseCommand = response.Headers.ToString();

                    var hResponse = (response as HttpWebResponse);
                    if (hResponse != null)
                    {
                        responseCommand = $"HTTP/{hResponse.ProtocolVersion} {(int)hResponse.StatusCode} " +
                            $"{hResponse.StatusDescription}\r\n{responseCommand}";
                    }
                    else
                        responseCommand = "HTTP/1.1 200 OK\r\n" + responseCommand;

                    string responseCookies = FormatResponseCookies(response);
                    if (!string.IsNullOrWhiteSpace(responseCookies))
                    {
                        string innerCookies = responseCommand.GetChild("Set-Cookie: ", '\r');

                        responseCommand = responseCommand
                            .Replace("Set-Cookie: " + innerCookies + "\r\n", responseCookies);
                    }

                    if (responseCommand.Contains("Content-Length"))
                    {
                        string contentLengthChild =
                            responseCommand.GetChild("Content-Length: ", '\r');

                        responseCommand = responseCommand.Replace(
                            contentLengthChild + "\r", (responseArgs.Payload?.Length ?? 0) + "\r");
                    }

                    byte[] responseCommandPayload = Encoding.ASCII.GetBytes(responseCommand);
                    clientStream.Write(responseCommandPayload,
                        0, responseCommandPayload.Length);

                    if (responseArgs.Payload != null)
                    {
                        clientStream.Write(responseArgs.Payload,
                            0, responseArgs.Payload.Length);
                    }
                }
            }
        }

        private static string[] GetRequestCommandArgs(string requestCommand)
        {
            string[] requestCommandArgs = requestCommand
                .GetParent("\r\n").Split(' ');

            string method = requestCommandArgs[0];
            string requestUrl = requestCommandArgs[1];

            requestCommandArgs[1] = requestUrl
                .Replace(":443", string.Empty);

            string host = requestCommand.GetChild("Host: ", '\r');
            if (requestCommand.StartsWith(method + " /") &&
                !string.IsNullOrWhiteSpace(host))
            {
                requestCommandArgs[1] =
                    "https://" + host + requestCommandArgs[1];
            }
            return requestCommandArgs;
        }
        private static HttpWebRequest CreateWebRequest(string requestCommand)
        {
            string[] requestCommandArgs =
                    GetRequestCommandArgs(requestCommand);

            string[] headerPairs = requestCommand
                .GetChild("\r\n").GetParent("\r\n\r\n")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var request = (HttpWebRequest)WebRequest.Create(requestCommandArgs[1]);
            request.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
            request.ProtocolVersion = new Version(1, 0);
            request.Method = requestCommandArgs[0];
            request.AllowAutoRedirect = false;
            request.KeepAlive = false;
            request.Proxy = null;

            foreach (string headerPair in headerPairs)
            {
                string[] values = headerPair.Split(new[] { ':', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                values[1] = headerPair.GetChild(values[0] + ": ");
                switch (values[0].ToLower())
                {
                    case "range":
                    case "expect":
                    case "keep-alive":
                    case "connection":
                    case "proxy-connection": break;

                    case "host": request.Host = values[1]; break;
                    case "accept": request.Accept = values[1]; break;
                    case "referer": request.Referer = values[1]; break;
                    case "user-agent": request.UserAgent = values[1]; break;
                    case "content-type": request.ContentType = values[1]; break;
                    case "content-length": request.ContentLength = long.Parse(values[1]); break;
                    case "if-modified-since": request.IfModifiedSince = DateTime.Parse(values[1].Split(';')[0], CultureInfo.InvariantCulture); break;

                    default: request.Headers[values[0]] = values[1]; break;
                }
            }
            return request;
        }
        private static byte[] ReadRequestStreamData(Stream clientStream, ref byte[] postRequestPayload)
        {
            using (var streamDataBuffer = new MemoryStream())
            {
                int streamByte = -1;
                int contentLength = 0;
                long lineStartPosition = 0;

                while ((streamByte = clientStream.ReadByte()) != -1)
                {
                    streamDataBuffer.WriteByte((byte)streamByte);
                    if (streamByte == 10)
                    {
                        byte[] lineBuffer = new byte[
                            streamDataBuffer.Length - lineStartPosition];

                        streamDataBuffer.Position = lineStartPosition;
                        streamDataBuffer.Read(lineBuffer, 0, lineBuffer.Length);

                        string line = Encoding.ASCII.GetString(lineBuffer);

                        if (line.StartsWith("Content-Length"))
                            contentLength = int.Parse(line.GetChild(": ", '\r'));

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (contentLength > 0)
                            {
                                postRequestPayload = new byte[contentLength];
                                clientStream.Read(postRequestPayload,
                                    0, postRequestPayload.Length);

                                streamDataBuffer.Write(postRequestPayload,
                                    0, postRequestPayload.Length);
                            }
                            break;
                        }
                        lineStartPosition = streamDataBuffer.Position;
                    }
                }
                return streamDataBuffer.ToArray();
            }
        }

        private static string FormatResponseCookies(WebResponse response)
        {
            string cookies = response.Headers["Set-Cookie"];
            if (string.IsNullOrWhiteSpace(cookies)) return string.Empty;

            string[] cookieValues = _cookieSplitter.Split(cookies);
            var cookieBuilder = new StringBuilder();

            foreach (var cookie in cookieValues)
                cookieBuilder.Append(string.Format("Set-Cookie: {0}\r\n", cookie));

            return cookieBuilder.ToString();
        }
        private static Stream GetSecureClientStream(string host, Stream innerStream)
        {
            try
            {
                var secureStream = new SslStream(innerStream, false);

                X509Certificate2 certificate =
                    Certificates.CreateCertificate(host.Split(':')[0]);

                secureStream.AuthenticateAsServer(certificate, false,
                    SslProtocols.Ssl2 | SslProtocols.Default, true);

                return secureStream;
            }
            catch { return innerStream; }
        }
    }
}