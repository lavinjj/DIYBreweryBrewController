using System;
using System.Threading;

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

        public void Start()
        {
            serviceThread = new Thread(Run);
            serviceThread.Start();
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