using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using CodingSmackdown.Sensors;
using CodingSmackdown.PID;

namespace CodingSmackdown.Services
{
    public class TemperatureControlService : ServiceBase
    {
        private readonly OutputHelper _outputHelper = null;

        private Thermistor _thermistor = null;
        private PIDController _pid = null;
        private double _output = 0;
        private int _windowSize = 5000;
        private long _windowStartTime;

        public TemperatureControlService(OutputHelper helper)
        {
            _outputHelper = helper;

            _thermistor = new Thermistor(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A0);
            _thermistor.VoltageReference = 3.3f;
            _thermistor.ResistanceReference = 1470000;

            _pid = new PIDController(PinManagement.currentTemperatureSensor, _output, PinManagement.setTemperature, 2, 5, 1, PIDController.PID_Direction.DIRECT);

            _windowSize = (int)SystemSettings.MinutesBetweenReadings;

            _pid.SetOutputLimits(0, _windowSize);

            _pid.Mode = PIDController.PID_Mode.AUTOMATIC;

            _windowStartTime = DateTime.Now.Ticks;
        }
        
        protected override void Run()
        {            
            while (true)
            {
                try
                {
                    double tempCelsius = _thermistor.GetTemperatureInC();

                    double tempFahrenheit = (tempCelsius * 1.8) + 32 + SystemSettings.TemperatureOffset;

                    // update the static values
                    PinManagement.currentTemperatureSensor = (float)tempFahrenheit;
                    PinManagement.temperatureCelsiusSensor = (float)tempCelsius;

                    _pid.Input = PinManagement.currentTemperatureSensor;
                    _pid.SetPoint = PinManagement.setTemperature;

                    _pid.Compute();

                    _output = _pid.Output;

                    if (PinManagement.heaterEngaged)
                    {
                        if ((DateTime.Now.Ticks - _windowStartTime) > _windowSize)
                        {
                            _windowStartTime += _windowSize;
                        }

                        if (_output < (DateTime.Now.Ticks - _windowStartTime))
                        {
                            PinManagement.heaterOnOffPort.Write(true);
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

                // Give up the clock so that the other threads can do their work
                Thread.Sleep(10);
            }
        }
    }
}
