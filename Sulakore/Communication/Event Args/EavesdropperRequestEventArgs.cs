using System.Net;
using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesdropperRequestEventArgs : CancelEventArgs
    {
        private byte[] _payload;
        public byte[] Payload
        {
            get { return _payload; }
            set
            {
                _payload = value;
                Request.ContentLength = value?.Length ?? 0;
            }
        }

        public WebRequest Request { get; set; }
        public byte[] ResponsePayload { get; set; }

        public EavesdropperRequestEventArgs(WebRequest request)
        {
            Request = request;
        }
    }
}