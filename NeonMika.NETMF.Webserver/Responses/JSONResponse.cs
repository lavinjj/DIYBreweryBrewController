using System;
using System.Net.Sockets;
using System.Text;
using FastloadMedia.NETMF.Http;
using NeonMika.Webserver.EventArgs;

namespace NeonMika.Webserver.Responses
{
    public class JSONResponse : Response
    {
        private JSONResponseCheck _CheckResponse;
        private JsonArray _Pairs;

        public JSONResponse(string name, JSONResponseCheck method)
            : base(name)
        {
            this._CheckResponse = method;
            _Pairs = new JsonArray();
        }

        /// <summary>
        /// Execute this to check if SendResponse should be executed
        /// </summary>
        /// <param name="RequestArguments">Event Args</param>
        /// <returns>True if URL refers to this method, otherwise false (false = SendRequest should not be executed) </returns>
        public override bool ConditionsCheckAndDataFill(RequestReceivedEventArgs RequestArguments)
        {
            _Pairs.Clear();
            if (RequestArguments.Request.URL == this.Name)
                return _CheckResponse(RequestArguments, _Pairs);
            else
                return false;
        }

        /// <summary>
        /// Sends XML to client
        /// </summary>
        /// <param name="requestArguments">Could be null</param>
        /// <returns>True if 200_OK was sent, otherwise false</returns>
        public override bool SendResponse(RequestReceivedEventArgs requestArguments)
        {
            String jsonResponse = String.Empty;

            jsonResponse = _Pairs.ToString();

            byte[] bytes = Encoding.UTF8.GetBytes(jsonResponse);

            int byteCount = bytes.Length;

            try
            {
                Send200_OK("application/json", byteCount, requestArguments.Client);
                requestArguments.Client.Send(bytes, byteCount, SocketFlags.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
    }
}