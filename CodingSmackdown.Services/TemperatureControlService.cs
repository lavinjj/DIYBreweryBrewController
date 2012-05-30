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
        private const double TABS = -273.15;
        /** Coefficients of Steinhart-Hart polynom. */
        private double[] a = {
	        2.900538876296411e-005,
	        4.503191323434938e-004,
	        -3.288268667833969e-005,
	        1.243331808293418e-006,
        };

        public TemperatureControlService(OutputHelper helper)
        {
            _outputHelper = helper;
        }

        private double poly(double x, int degree, double[] p) {
          double retval = 0.0;
          int i;

          for (i = degree; i >= 0; i--)
          {
              retval = retval * x + p[i];
          }

            return retval;
        }

        private double rtot(double r)
        {
            double ti;

            ti = poly(ElzeKool.exMath.Log(r), 3, a);
            ti = 1.0 / ti + TABS;
            return ti;
        }

        protected override void Run()
        {            
            while (true)
            {
                try
                {
                    float totalReadingSensor1 = 0;
                    float totalReadingSensor2 = 0;
                    // read the sensor 100 times and add up the value
                    // so we can get an average reading
                    for (int i = 0; i < 100; i++)
                    {
                        totalReadingSensor1 += PinManagement.temperatureSensorPort.Read();
                        Thread.Sleep(10);
                    }

                    // read the sensor 100 times and add up the value
                    // so we can get an average reading
                    for (int i = 0; i < 100; i++)
                    {
                        totalReadingSensor2 += PinManagement.temperatureSensorPort2.Read();
                        Thread.Sleep(10);
                    }

                    // calculate the temperature
                    float averageReadingSensor2 = totalReadingSensor2 / 100;
                    float milliVoltsSensor2 = averageReadingSensor2 * 3300 / 1023;
                    float tempCelsiusSensor2 = (milliVoltsSensor2 - 500) / 10;
                    float tempFahrenheitSensor2 = (float)(tempCelsiusSensor2 * 1.8) + 32 + SystemSettings.TemperatureOffset;
                    PinManagement.milliVoltsSensor2 = milliVoltsSensor2;
                    PinManagement.temperatureCelciusSensor2 = tempCelsiusSensor2;
                    PinManagement.currentTemperatureSensor2 = tempFahrenheitSensor2;

                    // calculate the temperature
                    double averageReading = totalReadingSensor1 / 100;
                    // double milliVolts = averageReading * SystemSettings.VoltageReference / 1023;
                    double milliVolts = averageReading * 3.3 / 1023;
                    // double vPad = SystemSettings.VoltageReference - milliVolts;
                    double vPad = 3.3 - milliVolts;
                    // double circuitCurrent = vPad / SystemSettings.PadResistance;
                    double circuitCurrent = vPad / 1470000;
                    double thermResistance = milliVolts / circuitCurrent;
                    PinManagement.resistanceSensor1 = thermResistance;
                    //double Temp;
                    //// We divide by our thermistor's resistance at 25C, in this case 10K
                    // Temp = ElzeKool.exMath.Log(thermResistance / SystemSettings.ResistanceRT);
                    //Temp = ElzeKool.exMath.Log(thermResistance / 1500000);
                    //Temp = 1 / (0.003354016 + (0.0002909670 * Temp) + (0.000001632136 * Temp * Temp) + (0.00000007192200 * Temp * Temp * Temp));
                    ////Temp = 1 / (0.003354016 + (0.0002744032 * Temp) + (0.0000001375492 * Temp * Temp * Temp));
                    double tempCelsius = rtot(thermResistance);
                    double tempFahrenheit = (tempCelsius * 1.8) + 32 + SystemSettings.TemperatureOffset;
                    // update the static values
                    PinManagement.milliVoltsSensor1 = (float)milliVolts;
                    PinManagement.currentTemperatureSensor1 = (float)tempFahrenheit;
                    PinManagement.temperatureCelciusSensor1 = (float)tempCelsius;
                    // calculate the difference between the set temperature and current temperature
                    float temperatureDifference = (float)System.Math.Abs(PinManagement.setTemperature - PinManagement.currentTemperatureSensor1);
                    // if the heater is engaged and the current temeprature is less than the set temperature and 
                    // the difference is greater than our threshold value
                    // make sure the heater is turned on by setting the heater port high
                    if ((PinManagement.heaterEngaged) && (PinManagement.currentTemperatureSensor1 < PinManagement.setTemperature) && (temperatureDifference > SystemSettings.TemperatureHeaterOffset))
                    {
                        PinManagement.heaterOnOffPort.Write(true);
                        //// change the duty cycle based on the temperature difference
                        //if (temperatureDifference > 10)
                        //{
                        //    // if the temperature difference is greater than 10 degrees
                        //    // set the port to 100% duty cycle always on
                        //    PinManagement.heaterOnOffPort.SetDutyCycle(100);
                        //    _outputHelper.DisplayText("Duty Cycle 100%");
                        //}
                        //else if ((temperatureDifference < 10) && (temperatureDifference > 2))
                        //{
                        //    // if the temperature difference is between 10 to 2 degrees
                        //    // set the port to 50% duty cycle on 50% of the time
                        //    PinManagement.heaterOnOffPort.SetDutyCycle(50);
                        //    _outputHelper.DisplayText("Duty Cycle 50%");
                        //}
                        //else
                        //{
                        //    // if the temperature is within 2 degrees or less
                        //    // set the port to 25% duty cycle alwayas on
                        //    PinManagement.heaterOnOffPort.SetDutyCycle(25);
                        //    _outputHelper.DisplayText("Duty Cycle 25%");
                        //}
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
                        //PinManagement.heaterOnOffPort.SetDutyCycle(0);
                        PinManagement.heaterOnOffPort.Write(false);
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
                    // display differences
                    _outputHelper.DisplaySensorTemperatures();
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
