using System;
using System.Net.Sockets;

namespace NeonMika.Webserver.EventArgs
{
    public class RequestReceivedEventArgs
    {
        private int byteCount;
        private Socket client;
        private DateTime receiveTime;
        private Request request;

        public RequestReceivedEventArgs(Request request, Socket client, int byteCount)
        {
            this.request = request;
            this.client = client;
            this.byteCount = byteCount;

            this.receiveTime = DateTime.Now;
        }

        public int ByteCount
        {
            get { return byteCount; }
        }

        public Socket Client
        {
            get { return client; }
        }

        public DateTime ReceiveTime
        {
            get { return receiveTime; }
        }

        public Request Request
        {
            get { return request; }
        }
    }
}