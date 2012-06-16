using System;
using System.IO;
using System.Net.Sockets;
using NeonMika.Webserver.EventArgs;

namespace NeonMika.Webserver.Responses
{
    /// <summary>
    /// Standard response sending file to client
    /// </summary>
    public class FileResponse : Response
    {
        public FileResponse(String name = "FileResponse")
            : base(name)
        { }

        public override bool ConditionsCheckAndDataFill(RequestReceivedEventArgs RequestArguments)
        {
            return true;
        }

        public override bool SendResponse(RequestReceivedEventArgs RequestArguments)
        {
            string url = String.Empty;

            string[] urlParts = RequestArguments.Request.URL.Split('/');
            bool addBackSlash = false;

            foreach (string part in urlParts)
            {
                if (addBackSlash)
                {
                    url = url + @"\" + part;
                }
                else
                {
                    url = part;
                    addBackSlash = true;
                }
            }

            string filePath = Settings.ROOT_PATH + url;

            //File found check
            try
            {
                if (!File.Exists(filePath))
                {
                    Send404_NotFound(RequestArguments.Client);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            string mType = MimeType(filePath);

            //File sending
            using (FileStream inputStream = new FileStream(filePath, FileMode.Open))
            {
                Send200_OK(mType, (int)inputStream.Length, RequestArguments.Client);

                byte[] readBuffer = new byte[Settings.FILE_BUFFERSIZE];
                int sentBytes = 0;

                //Sending parts in size of "Settings.FILE_BUFFERSIZE"
                while (sentBytes < inputStream.Length)
                {
                    int bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length);
                    try
                    {
                        sentBytes += RequestArguments.Client.Send(readBuffer, bytesRead, SocketFlags.None);
                    }
                    catch (Exception ex) //ToDo: Error code if client closes browser during file transport
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }

            return true;
        }
    }
}