using System;
using CodingSmackdown.Services;
using FastloadMedia.NETMF.Http;
using NeonMika.Webserver.EventArgs;

namespace CodingSmackdown.BrewController
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
                settingsValues.Add("kpValue", settings.PIDKp);
                settingsValues.Add("kiValue", settings.PIDKi);
                settingsValues.Add("kdValue", settings.PIDKd);

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