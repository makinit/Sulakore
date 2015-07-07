using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

namespace Sulakore.Communication
{
    public static class Eavesdropper
    {
        private static TcpListener _listener;

        private static readonly Regex _cookieSplitter;
        private static readonly CertificateManager _certManager;
        private static readonly List<Task> _handleClientRequestTasks;
        private static readonly object _initiateLock, _terminateLock;

        private static readonly byte[] _fakeOkResponse = { 72, 84, 84, 80, 47, 49, 46, 48, 32, 50, 48, 48, 13, 10, 13, 10 };

        public static event EventHandler<EavesdropperRequestEventArgs> EavesdropperRequest;
        private static void OnEavesdropperRequest(EavesdropperRequestEventArgs e)
        {
            EventHandler<EavesdropperRequestEventArgs> handler = EavesdropperRequest;
            if (handler != null) handler(null, e);
        }

        public static event EventHandler<EavesdropperResponseEventArgs> EavesdropperResponse;
        private static void OnEavesdropperResponse(EavesdropperResponseEventArgs e)
        {
            EventHandler<EavesdropperResponseEventArgs> handler = EavesdropperResponse;
            if (handler != null) handler(null, e);
        }

        public static bool IsSslSupported
        {
            get { return NativeMethods.IsSslSupported; }
            set { NativeMethods.IsSslSupported = value; }
        }
        public static int ListenPort { get; private set; }
        public static bool IsRunning { get; private set; }
        public static bool IsProxyRegistered
        {
            get { return NativeMethods.IsProxyRegistered; }
        }

        static Eavesdropper()
        {
            _certManager = new CertificateManager("Eavesdropper",
                "Eavesdropper Root Certificate Authority");

            _initiateLock = new object();
            _terminateLock = new object();
            _cookieSplitter = new Regex(@",(?! )");
            _handleClientRequestTasks = new List<Task>();
        }

        public static void Terminate()
        {
            if (!IsRunning || !Monitor.TryEnter(_terminateLock))
                return;
            try
            {
                IsRunning = false;

                if (_listener != null)
                    _listener.Stop();
            }
            finally
            {
                Monitor.Exit(_terminateLock);
                NativeMethods.TerminateProxy();
            }
        }
        public static void Initiate(int port)
        {
            if (IsRunning || !Monitor.TryEnter(_initiateLock))
                return;
            try
            {
                IsRunning = true;

                ListenPort = port;
                NativeMethods.InitiateProxy(port);

                Task.Factory.StartNew(ReadClientRequestLoop,
                    TaskCreationOptions.LongRunning);
            }
            catch
            {
                ListenPort = 0;
                NativeMethods.TerminateProxy();

                IsRunning = false;
            }
            finally { Monitor.Exit(_initiateLock); }
        }

        public static bool DestroyTrustedRootCertificate()
        {
            return _certManager.DestroyTrustedRootCertificate();
        }
        public static bool CreateTrustedRootCertificate()
        {
            return _certManager.CreateTrustedRootCertificate();
        }

        private static void ReadClientRequestLoop()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, ListenPort);
                _listener.Start();

                while (IsRunning)
                {
                    if (_handleClientRequestTasks.Count >= 3)
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
                    catch { if (!IsRunning) break; }
                }
            }
            finally { _handleClientRequestTasks.Clear(); }
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
                    socket.Send(_fakeOkResponse);
                    if (!requestCommand.Contains(":443")) return;

                    string host = requestCommand.GetChild("Host: ", '\r');
                    clientStream = GetSecureClientStream(host, clientStream);

                    requestCommandPayload =
                        ReadRequestStreamData(clientStream, ref postRequestPayload);

                    if (requestCommandPayload.Length < 1) return;
                    requestCommand = Encoding.ASCII.GetString(requestCommandPayload);
                }

                HttpWebRequest request = CreateWebRequest(requestCommand);
                request.Headers["Cache-Control"] = "no-cache, no-store";

                var requestArgs = new EavesdropperRequestEventArgs(request);
                requestArgs.Payload = postRequestPayload;

                OnEavesdropperRequest(requestArgs);
                if (requestArgs.Payload != null && requestArgs.Payload.Length > 0 && !requestArgs.Cancel)
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(requestArgs.Payload,
                            0, requestArgs.Payload.Length);
                    }
                }

                HandleClientResponse(requestArgs, clientStream);
            }
            finally
            {
                if (clientStream != null)
                    clientStream.Dispose();
            }
        }
        private static void HandleClientResponse(EavesdropperRequestEventArgs requestArgs, Stream clientStream)
        {
            WebResponse webResponse = null;
            try { webResponse = requestArgs.Request.GetResponse(); }
            catch (WebException e) { webResponse = e.Response; }

            if (webResponse == null) return;
            using (var response = (HttpWebResponse)webResponse)
            {
                if (requestArgs.Cancel) return;
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
                    string responseCommand =
                        string.Format("HTTP/{0} {1} {2}\r\n{3}",
                        response.ProtocolVersion, (int)response.StatusCode, response.StatusDescription, response.Headers);

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
                            contentLengthChild + "\r", responseArgs.Payload.Length + "\r");
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
                    case "if-modified-since": request.IfModifiedSince = DateTime.Parse(values[1]); break;

                    default: request.Headers[values[0]] = values[1]; break;
                }
            }
            return request;
        }
        private static byte[] ReadRequestStreamData(Stream clientStream, ref byte[] postRequestPayload)
        {
            using (var streamDataBuffer = new MemoryStream())
            {
                int contentLength = 0;
                long lineStartPosition = 0;
                while (true)
                {
                    int streamByte = clientStream.ReadByte();
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

        private static string FormatResponseCookies(HttpWebResponse response)
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
            var secureStream = new SslStream(innerStream, false);
            X509Certificate2 certificate = _certManager.CreateCertificate(host);

            secureStream.AuthenticateAsServer(certificate, false,
                SslProtocols.Ssl2 | SslProtocols.Default, true);

            return secureStream;
        }
    }
}