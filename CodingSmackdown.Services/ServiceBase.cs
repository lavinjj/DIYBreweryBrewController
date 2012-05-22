using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CodingSmackdown.Services
{
    public class ServiceBase
    {
        private Thread serviceThread;
        private Settings systemSettings;

        public Settings SystemSettings
        {
          get { return systemSettings; }
          set { systemSettings = value; }
        }

        public void Start()
        {
            serviceThread = new Thread(Run);
            serviceThread.Start();
        }

        /// <summary>
        /// Close this class
        /// </summary>
        public void Dispose()
        {
            // Close Thread
            try 
            {
                serviceThread.Abort();
                DateTime timeoutAt = DateTime.Now.AddSeconds(10);
                while (serviceThread.ThreadState != ThreadState.Aborted
                        && DateTime.Now < timeoutAt) Thread.Sleep(100);
            }
            catch { }
            serviceThread = null;
        }

        /// <summary>
        /// Default Run Loop, Override to provide class level
        /// functionality
        /// </summary>
        protected virtual void Run()
        {
            while (true)
            {
                Thread.Sleep((int)SystemSettings.MinutesBetweenReadings);
            }
        }


    }
}
