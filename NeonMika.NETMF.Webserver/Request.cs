using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace NeonMika.Webserver
{
    //ToDo: Post-Values have to be copied into hashtable "postContent"

    public class Request : IDisposable
    {
        protected string method;
        protected string url;
        protected Hashtable postArguments = new Hashtable();
        protected Hashtable getArguments = new Hashtable();

        /// <summary>
        /// Hashtable with all GET key-value pa in it
        /// </summary>
        public Hashtable GetArguments
        {
            get { return getArguments; }
            private set { getArguments = value; }
        }

        /// <summary>
        /// HTTP verb (Request method)
        /// </summary>
        public string Method
        {
            get { return method; }
            private set { method = value; }
        }

        /// <summary>
        /// URL of request without GET values
        /// </summary>
        public string URL
        {
            get { return url; }
            private set { url = value; }
        }

        /// <summary>
        /// ToDo: Post-Values have to be copied into hashtable
        /// Full POST line is saved to "post"-key
        /// </summary>
        public Hashtable PostContent
        {
            get { return postArguments; }
        }

        /// <summary>
        /// Creates request
        /// </summary>
        /// <param name="Data">Input from network</param>
        public Request(char[] Data)
        {
            ProcessRequest(Data);
        }
       
        /// <summary>
        /// Sets up the request
        /// </summary>
        /// <param name="data">Input from network</param>
        private void ProcessRequest(char[] data)
        {
            string content = new string(data);
            string firstLine = content.Substring(0, content.IndexOf('\n'));

            // Parse the first line of the request: "GET /path/ HTTP/1.1"
            string[] firstLineWords = firstLine.Split(' ');
            method = firstLineWords[0];
            string[] urlAndGets = firstLineWords[1].Split('?');
            url = urlAndGets[0].Substring(1); // Substring to ignore the '/'

            if (urlAndGets.Length > 1)
            {
                FillGETHashtable(urlAndGets[1]);
            }

            if (method.ToUpper() == "POST") // TODO!
            {
                int lastLine = content.LastIndexOf('\n');
                postArguments.Clear();
                postArguments.Add("post", content.Substring(lastLine + 1, content.Length - lastLine));
            }
            else
                postArguments = null;

            // Could look for any further headers in other lines of the request if required (e.g. User-Agent, Cookie)
        }

        /// <summary>
        /// builds arguments hash table
        /// </summary>
        /// <param name="value"></param>
        private void FillGETHashtable(string url)
        {
            getArguments = new Hashtable();

            string[] urlArguments = url.Split('&');
            string[] keyValuePair;

            for (int i = 0; i < urlArguments.Length; i++)
            {
                keyValuePair = urlArguments[i].Split('=');
                getArguments.Add(keyValuePair[0], keyValuePair[1]);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(postArguments != null)
                postArguments.Clear();
            
            if(getArguments != null)
                getArguments.Clear();
        }

        #endregion
    }
}