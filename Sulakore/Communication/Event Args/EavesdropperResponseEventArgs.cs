using System.Net;
using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesdropperResponseEventArgs : CancelEventArgs
    {
        public byte[] Payload { get; set; }
        public HttpWebResponse Response { get; private set; }

        public EavesdropperResponseEventArgs(HttpWebResponse response)
        {
            Response = response;
        }
    }
}