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
    public class NTPTimeService : ServiceBase
    {
        protected override void Run()
        {
            while (true)
            {
                try
                {
                    DateTime recordTime = NTPTime(SystemSettings.NTPServerName, SystemSettings.TimeZoneOffset);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                Thread.Sleep(SystemSettings.MinutesBetweenNTPUpdate);
            }
        }

        /// <summary>
        /// Try to update both system and RTC time using the NTP protocol
        /// </summary>
        /// <param name="TimeServer">Time server to use, ex: pool.ntp.org</param>
        /// <param name="GmtOffset">GMT offset in minutes, ex: -240</param>
        /// <returns>Returns true if successful</returns>
        public DateTime NTPTime(string TimeServer, int GmtOffset = 0)
        {
            Socket s = null;
            DateTime resultTime = DateTime.Now;
            try
            {
                EndPoint rep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 123);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] ntpData = new byte[48];
                Array.Clear(ntpData, 0, 48);
                ntpData[0] = 0x1B; // Set protocol version
                s.SendTo(ntpData, rep); // Send Request   
                if (s.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) // Waiting an answer for 30s, if nothing: timeout
                {
                    s.ReceiveFrom(ntpData, ref rep); // Receive Time
                    byte offsetTransmitTime = 40;
                    ulong intpart = 0;
                    ulong fractpart = 0;
                    for (int i = 0; i <= 3; i++) intpart = (intpart << 8) | ntpData[offsetTransmitTime + i];
                    for (int i = 4; i <= 7; i++) fractpart = (fractpart << 8) | ntpData[offsetTransmitTime + i];
                    ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
                    s.Close();
                    resultTime = new DateTime(1900, 1, 1) + TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
                    Utility.SetLocalTime(resultTime.AddMinutes(GmtOffset));
                }
                s.Close();
            }
            catch
            {
                try { s.Close(); }
                catch { }
            }
            return resultTime;
        }
    
    }
}
