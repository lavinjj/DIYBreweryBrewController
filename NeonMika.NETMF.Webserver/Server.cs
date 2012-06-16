using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Net.NetworkInformation;
using NeonMika.Webserver.EventArgs;
using NeonMika.Webserver.Responses;

namespace NeonMika.Webserver
{
    /// <summary>
    /// JSON Expansion methods have to be in this form
    /// </summary>
    /// <param name="e">Access to GET or POST arguments,...</param>
    /// <param name="results">This JsonArray gets converted into JSON on response</param>
    /// <returns>True if URL refers to this method, otherwise false (false = SendRequest should not be executed) </returns>
    public delegate bool JSONResponseCheck(RequestReceivedEventArgs e, JsonArray results);

    /// <summary>
    /// Use this for RequestReceived events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RequestReceivedHandler(object sender, RequestReceivedEventArgs e);

    public class Server
    {
        private readonly int _PortNumber = 80;
        private Socket _ListeningSocket = null;
        private Hashtable _Responses = new Hashtable();

        /// <summary>
        /// Creates an instance running in a separate thread
        /// </summary>
        /// <param name="portNumber">The port to listen</param>
        public Server(int portNumber = 80)
        {
            var interf = NetworkInterface.GetAllNetworkInterfaces()[0];

            System.Diagnostics.Debug.WriteLine("Webserver is running on " + interf.IPAddress + " /// DHCP: " + interf.IsDhcpEnabled);

            this._PortNumber = portNumber;

            ResponseListInitialize();

            _ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ListeningSocket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
            _ListeningSocket.Listen(10);

            var webserverThread = new Thread(WaitingForRequest);
            webserverThread.Start();
        }

        public EndPoint LocalIP
        {
            get
            {
                if (_ListeningSocket == null)
                    return null;
                else
                    return _ListeningSocket.LocalEndPoint;
            }
        }

        public EndPoint RemoteIP
        {
            get
            {
                if (_ListeningSocket == null)
                    return null;
                else
                    return _ListeningSocket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Adds a XMLResponse
        /// </summary>
        /// <param name="response">XMLResponse that has to be added</param>
        public void AddResponse(Response response)
        {
            if (!_Responses.Contains(response.Name))
            {
                _Responses.Add(response.Name, response);
            }
        }

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //---------------Webserver expansion---------------------------
        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //-------------------Basic methods-----------------------------
        /// <summary>
        /// Removes a XMLResponse
        /// </summary>
        /// <param name="ResponseName">XMLResponse that has to be deleted</param>
        public void RemoveResponse(String ResponseName)
        {
            if (_Responses.Contains(ResponseName))
            {
                _Responses.Remove(ResponseName);
            }
        }

        /// <summary>
        /// Example for web server expand method
        /// Call via http://servername/echo?value='echovalue'
        /// Submit a 'value' GET parameter
        /// </summary>
        /// <param name="e"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private bool Echo(RequestReceivedEventArgs e, JsonArray results)
        {
            if (e.Request.GetArguments.Contains("value") == true)
                results.Add(e.Request.GetArguments["value"]);
            else
                results.Add("No 'value'-parameter transmitted to server");
            return true;
        }

        /// <summary>
        /// Creates a response for a request and sends it
        /// </summary>
        /// <param name="e"></param>
        private void HandleRequest(RequestReceivedEventArgs e)
        {
            string ip = e.Client.RemoteEndPoint.ToString();

            Response response = null;

            if (_Responses.Contains(e.Request.URL))
            {
                response = (Response)_Responses[e.Request.URL];
            }
            else
            {
                response = (Response)_Responses["FileResponse"];
            }

            if (response != null)
            {
                if (response.ConditionsCheckAndDataFill(e))
                {
                    response.SendResponse(e);
                }
            }
        }

        /// <summary>
        /// Initialize the basic functionalities of the web server
        /// </summary>
        private void ResponseListInitialize()
        {
            //KEEP THIS FIRST
            AddResponse(new FileResponse());

            AddResponse(new JSONResponse("echo", new JSONResponseCheck(Echo)));
        }

        /// <summary>
        /// Waiting for client to connect.
        /// When bytes were read they get wrapped to a "Request" and packed into a "RequestReceivedEventArgs"
        /// </summary>
        private void WaitingForRequest()
        {
            while (true)
            {
                try
                {
                    using (Socket clientSocket = _ListeningSocket.Accept())
                    {
                        int availableBytes = 0;

                        //if client connects but sends no data in the first milliseconds
                        for (int receiveTry = 0; receiveTry < 3; receiveTry++)
                        {
                            if ((availableBytes = clientSocket.Available) == 0)
                                Thread.Sleep(10);
                            else
                                break;
                        }

                        Thread.Sleep(5);

                        int newAvBytes;

                        //if not all incoming bytes were received by the socket
                        while (true)
                        {
                            newAvBytes = clientSocket.Available - availableBytes;

                            if (newAvBytes == 0)
                                break;

                            availableBytes += newAvBytes;
                            newAvBytes = 0;
                            Thread.Sleep(5);
                        }

                        //ignore requests that are too big
                        if (availableBytes < Settings.MAX_REQUESTSIZE && availableBytes > 0)
                        {
                            byte[] buffer = new byte[availableBytes];
                            int readByteCount = clientSocket.Receive(buffer, availableBytes, SocketFlags.None);

                            //request created, checking the response possibilities
                            using (Request tempRequest = new Request(Encoding.UTF8.GetChars(buffer)))
                            {
                                RequestReceivedEventArgs e = new RequestReceivedEventArgs(tempRequest, clientSocket, availableBytes);

                                HandleRequest(e);
                            }
                        }

                        clientSocket.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //-----------------------EXPAND this methods-------------------
        //-------------------------------------------------------------
        //---------------------Expansion Methods-----------------------
        //-------------------------------------------------------------
        //----------Look at the echo method for xml example------------
    }
}