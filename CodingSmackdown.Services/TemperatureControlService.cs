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
                    //float averageReading = totalReading / 100;
                    //float milliVolts = averageReading * 3300 / 1023;
                    //float tempCelsius = (milliVolts - 500) / 10;
                    //float tempFahrenheit = (float)(tempCelsius * 1.8) + 32;

                    // calculate the temperature
                    double averageReading = totalReading / 100;
                    // double milliVolts = averageReading * SystemSettings.VoltageReference / 1023;
                    double milliVolts = averageReading * 3.3 / 1023;
                    // double vPad = SystemSettings.VoltageReference - milliVolts;
                    double vPad = 3.3 - milliVolts;
                    // double circuitCurrent = vPad / SystemSettings.PadResistance;
                    double circuitCurrent = vPad / 1470000;
                    double thermResistance = milliVolts / circuitCurrent;
                    double Temp;
                    //// We divide by our thermistor's resistance at 25C, in this case 10K
                    // Temp = ElzeKool.exMath.Log(thermResistance / SystemSettings.ResistanceRT);
                    Temp = ElzeKool.exMath.Log(thermResistance / 1500000);
                    Temp = 1 / (0.003354016 + (0.0002909670 * Temp) + (0.000001632136 * Temp * Temp) + (0.00000007192200 * Temp * Temp * Temp));
                    ////Temp = 1 / (0.003354016 + (0.0002744032 * Temp) + (0.0000001375492 * Temp * Temp * Temp));
                    double tempCelsius = Temp - 274.15;
                    double tempFahrenheit = (tempCelsius * 1.8) + 32;
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
                        // change the duty cycle based on the temperature difference
                        if (temperatureDifference > 10)
                        {
                            // if the temperature difference is greater than 10 degrees
                            // set the port to 100% duty cycle always on
                            PinManagement.heaterOnOffPort.SetDutyCycle(100);
                            _outputHelper.DisplayText("Duty Cycle 100%");
                        }
                        else if ((temperatureDifference < 10) && (temperatureDifference > 2))
                        {
                            // if the temperature difference is between 10 to 2 degrees
                            // set the port to 50% duty cycle on 50% of the time
                            PinManagement.heaterOnOffPort.SetDutyCycle(50);
                            _outputHelper.DisplayText("Duty Cycle 50%");
                        }
                        else
                        {
                            // if the temperature is within 2 degrees or less
                            // set the port to 25% duty cycle alwayas on
                            PinManagement.heaterOnOffPort.SetDutyCycle(25);
                            _outputHelper.DisplayText("Duty Cycle 25%");
                        }
                        // set the heating flag to on
                        PinManagement.isHeating = true;
                        // display heat is on
                        _outputHelper.DisplayText("Heat On");
                        // reset the buzzer flag
                        PinManagement.alarmSounded = false;
                    }
                    else
                    {
                        // the heater is not engaged or the current temperature is greater or equal to the set temperature
                        // turn off the heater is turned off by setting the heater port loq
                        PinManagement.heaterOnOffPort.SetDutyCycle(0);
                        // set the heating flag to off
                        PinManagement.isHeating = false;
                        // display heat is on
                        _outputHelper.DisplayText("Heat Off");
                        // if we haven't sounded the alarm to indicate we have reached the set temperature
                        // and we are within the turn off threshold then sound the alarm
                        if (!PinManagement.alarmSounded)
                        {
                            PinManagement.buzzerPort.Write(true);
                            Thread.Sleep(2000);
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
