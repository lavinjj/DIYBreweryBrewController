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
    public class GetTemperatureMethod
    {
        public static bool GetTemperature(RequestReceivedEventArgs e, JsonArray h)
        {
            try
            {
                DateTime timeOfReading = DateTime.Now;

                JsonObject reading = new JsonObject();
                reading.Add("timeOfReading", timeOfReading.ToString());
                reading.Add("sensorMilliVolts", PinManagement.milliVolts.ToString("f4"));
                reading.Add("temperatureCelsius", PinManagement.temperatureCelcius.ToString("f2"));
                reading.Add("temperatureFahrenheit", PinManagement.currentTemperature.ToString("f2"));

                h.Add(reading);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return true;
        }
    }
}
