using System;
using CodingSmackdown.Services;
using FastloadMedia.NETMF.Http;
using NeonMika.Webserver.EventArgs;

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
                reading.Add("temperatureCelsius", PinManagement.temperatureCelsiusSensor.ToString("f2"));
                reading.Add("temperatureFahrenheit", PinManagement.currentTemperatureSensor.ToString("f2"));
                reading.Add("isHeating", PinManagement.isHeating.ToString());

                if ((PinManagement.mashSteps != null) && (PinManagement.mashSteps.CurrentStep != null))
                {
                    reading.Add("currentMashStep", PinManagement.mashSteps.CurrentStep.StepNumber.ToString());
                    reading.Add("currentMashTemp", PinManagement.mashSteps.CurrentStep.Temperature.ToString("f2"));
                    reading.Add("currentMashTime", PinManagement.mashSteps.CurrentStep.Time.ToString());
                }

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