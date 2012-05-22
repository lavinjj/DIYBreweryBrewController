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
    public class TemperatureControlService : ServiceBase
    {
        private readonly OutputHelper _outputHelper = null;

        public TemperatureControlService(OutputHelper helper)
        {
            _outputHelper = helper;
        }

        protected override void Run()
        {            
            while (true)
            {
                try
                {
                    float totalReading = 0;
                    // read the sensor 100 times and add up the value
                    // so we can get an average reading
                    for (int i = 0; i < 100; i++)
                    {
                        totalReading += PinManagement.temperatureSensorPort.Read();
                        Thread.Sleep(10);
                    }
                    // calculate the temperature
                    double averageReading = totalReading / 100;
                    double milliVolts = averageReading * SystemSettings.VoltageReference / 1024;
                    double vPad = SystemSettings.VoltageReference - milliVolts;
                    double circuitCurrent = vPad / SystemSettings.PadResistance;
                    double thermResistance = milliVolts / circuitCurrent;
                    double Temp;
                    // We divide by our thermistor's resistance at 25C, in this case 10K
                    Temp = ElzeKool.exMath.Log(thermResistance / SystemSettings.ResistanceRT);
                    Temp = 1 / (SystemSettings.CoefficientA + (SystemSettings.CoefficientB * Temp) + (SystemSettings.CoefficientC * Temp * Temp) + (SystemSettings.CoefficientD * Temp * Temp * Temp));
                    double tempCelsius = Temp - 273.15;
                    double tempFahrenheit = ((tempCelsius * 9) / 5) + 32;
                    // update the static values
                    PinManagement.milliVolts = (float)milliVolts;
                    PinManagement.currentTemperature = (float)tempFahrenheit;
                    PinManagement.temperatureCelcius = (float)tempCelsius;
                    // calculate the difference between the set temperature and current temperature
                    float temperatureDifference = (float)System.Math.Abs(PinManagement.setTemperature - PinManagement.currentTemperature);
                    // if the heater is engaged and the current temeprature is less than the set temperature and 
                    // the difference is greater than our threshold value
                    // make sure the heater is turned on by setting the heater port high
                    if ((PinManagement.heaterEngaged) && (PinManagement.currentTemperature < PinManagement.setTemperature) && (temperatureDifference > SystemSettings.TemperatureHeaterOffset))
                    {
                        PinManagement.heaterOnOffPort.Write(PinManagement.heaterEngaged);
                        // set the heating flag to on
                        PinManagement.isHeating = true;
                        // display heat is on
                        _outputHelper.DisplayText("Heat On");
                    }
                    else
                    {
                        // the heater is not engaged or the current temperature is greater or equal to the set temperature
                        // turn off the heater is turned off by setting the heater port loq
                        PinManagement.heaterOnOffPort.Write(false);
                        // set the heating flag to off
                        PinManagement.isHeating = false;
                        // display heat is on
                        _outputHelper.DisplayText("Heat Off");
                        // if we haven't sounded the alarm to indicate we have reached the set temperature
                        // and we are within the turn off threshold then sound the alarm
                        if ((!PinManagement.alarmSounded) && (temperatureDifference < SystemSettings.TemperatureHeaterOffset))
                        {
                            PinManagement.buzzerPort.Write(true);
                            Thread.Sleep(1000);
                            PinManagement.buzzerPort.Write(false);
                            PinManagement.alarmSounded = true;
                        }
                    }
                    // update the log file
                    _outputHelper.UpdateTemperatureLogFile();
                    // update the LCD display
                    _outputHelper.UpdateTemeratureDisplay();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                // wait til next reading cycle
                Thread.Sleep((int)SystemSettings.MinutesBetweenReadings);
            }
        }
    }
}
