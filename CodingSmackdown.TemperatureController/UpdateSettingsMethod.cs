using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using NeonMika.Webserver.EventArgs;
using System.Collections;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using CodingSmackdown.Services;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Net.NetworkInformation;


namespace CodingSmackdown.TemperatureController
{
    public class UpdateSettingsMethod
    {
        public static bool UpdateSettings(RequestReceivedEventArgs e, JsonArray h)
        {
            try
            {
                Settings settings = new Settings();
                settings.loadSettings();
                float tempValue = 0.0F;

                if (e.Request.GetArguments.Contains("minutesBetweenReadings"))
                {
                    Settings.TryParseFloat(e.Request.GetArguments["minutesBetweenReadings"].ToString(), out tempValue);
                    settings.MinutesBetweenReadings = tempValue;
                }
                if (e.Request.GetArguments.Contains("historyFilename"))
                {
                    settings.HistoryFilename = e.Request.GetArguments["historyFilename"].ToString();
                }
                if (e.Request.GetArguments.Contains("ntpServerName"))
                {
                    settings.NTPServerName = e.Request.GetArguments["ntpServerName"].ToString();
                }
                if (e.Request.GetArguments.Contains("minutesBetweenNTPUpdate"))
                {
                    settings.MinutesBetweenNTPUpdate = Convert.ToInt32(e.Request.GetArguments["minutesBetweenNTPUpdate"].ToString());
                }
                if (e.Request.GetArguments.Contains("timeZoneOffset"))
                {
                    settings.TimeZoneOffset = Convert.ToInt32(e.Request.GetArguments["timeZoneOffset"].ToString());
                }
                if (e.Request.GetArguments.Contains("netbiosName"))
                {
                    settings.NetBiosName = e.Request.GetArguments["netbiosName"].ToString();
                }
                if (e.Request.GetArguments.Contains("temperatureOffset"))
                {
                    Settings.TryParseFloat(e.Request.GetArguments["temperatureOffset"].ToString(), out tempValue);
                    settings.TemperatureOffset = tempValue;
                }
                if (e.Request.GetArguments.Contains("temperatureHeaterOffset"))
                {
                    Settings.TryParseFloat(e.Request.GetArguments["temperatureHeaterOffset"].ToString(), out tempValue);
                    settings.TemperatureHeaterOffset = tempValue;
                }
                if (e.Request.GetArguments.Contains("enableDHCP"))
                {
                    if(e.Request.GetArguments["enableDHCP"].ToString().ToLower().Equals("on"))
                    {
                        settings.EnableDHCP = true;
                    }
                    else
                    {
                        settings.EnableDHCP = false;
                    }
                }
                if (e.Request.GetArguments.Contains("staticIPAddress"))
                {
                    settings.StaticIPAddress = e.Request.GetArguments["staticIPAddress"].ToString();
                }
                if (e.Request.GetArguments.Contains("subnetMask"))
                {
                    settings.SubnetMask = e.Request.GetArguments["subnetMask"].ToString();
                }
                if (e.Request.GetArguments.Contains("defaultGateway"))
                {
                    settings.DefaultGateway = e.Request.GetArguments["defaultGateway"].ToString();
                }
                if (e.Request.GetArguments.Contains("primaryDNSAddress"))
                {
                    settings.PrimaryDNSAddress = e.Request.GetArguments["primaryDNSAddress"].ToString();
                }
                if (e.Request.GetArguments.Contains("secondaryDNSAddress"))
                {
                    settings.SecondaryDNSAddress = e.Request.GetArguments["secondaryDNSAddress"].ToString();
                }
                // save the new device settings
                settings.saveSettings();
                // send back an ok response
                h.Add("OK");
                // spool up a thread that will sleep for 10 seconds and then reboot the Netduino
                var rebootThread = new Thread(new ThreadStart(delegate()
                {
                    Thread.Sleep(10000);
                    PowerState.RebootDevice(false, 30000);
                }));
                rebootThread.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                h.Add("ERROR");
                h.Add(ex.Message);
            }

            return true;
        }

        public static void UpdateNetworkConfiguration(Settings settings)
        {
            var interf = NetworkInterface.GetAllNetworkInterfaces()[0];

            if (settings.EnableDHCP)
            {
                interf.EnableDynamicDns();
                interf.EnableDhcp();
                //interf.ReleaseDhcpLease();
                //interf.RenewDhcpLease();
            }
            else
            {
                interf.EnableStaticIP(settings.StaticIPAddress, settings.SubnetMask, settings.DefaultGateway);
                interf.EnableStaticDns(new string[] { settings.PrimaryDNSAddress, settings.SecondaryDNSAddress });
            }

            interf = NetworkInterface.GetAllNetworkInterfaces()[0];
            int count = 0;

            while ((interf.IPAddress.Equals("0.0.0.0")) && (count < 100))
            {
                Thread.Sleep(1000);

                interf = NetworkInterface.GetAllNetworkInterfaces()[0];
                count++;
            }

            System.Diagnostics.Debug.WriteLine("Webserver is running on " + interf.IPAddress + " /// DHCP: " + interf.IsDhcpEnabled);

        }
    }
}
