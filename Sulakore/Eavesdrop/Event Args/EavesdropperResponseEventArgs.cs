using System.Net;
using System.ComponentModel;

namespace Eavesdrop
{
    public class EavesdropperResponseEventArgs : CancelEventArgs
    {
        public byte[] Payload { get; set; }
        public WebResponse Response { get; set; }

        public EavesdropperResponseEventArgs(WebResponse response)
        {
            Response = response;
        }
    }
}