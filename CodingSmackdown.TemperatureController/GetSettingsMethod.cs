using System;
using Microsoft.SPOT;
using NeonMika.Webserver.EventArgs;
using System.Collections;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using CodingSmackdown.Services;
using FastloadMedia.NETMF.Http;

namespace CodingSmackdown.TemperatureController
{
    public class GetSettingsMethod
    {
        public static bool GetSettings(RequestReceivedEventArgs e, JsonArray h)
        {
            try
            {
                Settings settings = new Settings();
                settings.loadSettings();

                JsonObject settingsValues = new JsonObject();
                settingsValues.Add("timeZoneOffset", settings.TimeZoneOffset.ToString());
                float value = settings.MinutesBetweenReadings / 60000;
                settingsValues.Add("minutesBetweenReadings", value.ToString("f2"));
                settingsValues.Add("temperatureOffset", settings.TemperatureOffset.ToString("f2"));
                settingsValues.Add("temperatureHeaterOffset", settings.TemperatureHeaterOffset.ToString("f2"));
                settingsValues.Add("historyFilename", settings.HistoryFilename);
                settingsValues.Add("ntpServerName", settings.NTPServerName);
                int tempValue = settings.MinutesBetweenNTPUpdate / 60000;
                settingsValues.Add("minutesBetweenNTPUpdate", tempValue.ToString());
                settingsValues.Add("netbiosName", settings.NetBiosName);
                settingsValues.Add("enableDHCP", settings.EnableDHCP.ToString());
                settingsValues.Add("staticIPAddress", settings.StaticIPAddress);
                settingsValues.Add("subnetMask", settings.SubnetMask);
                settingsValues.Add("defaultGateway", settings.DefaultGateway);
                settingsValues.Add("primaryDNSAddress", settings.PrimaryDNSAddress);
                settingsValues.Add("secondaryDNSAddress", settings.SecondaryDNSAddress);
                settingsValues.Add("voltageReference", settings.VoltageReference);
                settingsValues.Add("padResistance", settings.PadResistance);
                settingsValues.Add("resistanceRT", settings.ResistanceRT);
                settingsValues.Add("coefficientA", settings.CoefficientA);
                settingsValues.Add("coefficientB", settings.CoefficientB);
                settingsValues.Add("coefficientC", settings.CoefficientC);
                settingsValues.Add("coefficientD", settings.CoefficientD);

                h.Add(settingsValues);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return true;
        }
    }
}
