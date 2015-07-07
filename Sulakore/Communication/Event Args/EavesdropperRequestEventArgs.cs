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

                if (value != null)
                    Request.ContentLength = value.Length;
            }
        }

        public HttpWebRequest Request { get; private set; }

        public EavesdropperRequestEventArgs(HttpWebRequest request)
        {
            Request = request;
        }
    }
}