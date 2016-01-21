using System.Net;
using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesdropperResponseEventArgs : CancelEventArgs
    {
        public byte[] Payload { get; set; }
        public bool IsResponseOK { get; set; }
        public WebResponse Response { get; set; }

        public EavesdropperResponseEventArgs(WebResponse response)
        {
            Response = response;

            var httpResponse = (response as HttpWebResponse);
            if (httpResponse != null)
            {
                IsResponseOK =
                    (httpResponse.StatusCode == HttpStatusCode.OK);
            }
        }
    }
}