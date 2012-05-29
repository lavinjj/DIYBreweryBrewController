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
                reading.Add("sensorMilliVolts", PinManagement.milliVoltsSensor1.ToString("f4"));
                reading.Add("temperatureCelsius", PinManagement.temperatureCelciusSensor1.ToString("f2"));
                reading.Add("temperatureFahrenheit", PinManagement.currentTemperatureSensor1.ToString("f2"));
                reading.Add("sensorMilliVolts2", PinManagement.milliVoltsSensor2.ToString("f4"));
                reading.Add("temperatureCelsius2", PinManagement.temperatureCelciusSensor2.ToString("f2"));
                reading.Add("temperatureFahrenheit2", PinManagement.currentTemperatureSensor2.ToString("f2"));

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
