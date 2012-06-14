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
        private PIDAutoTune _pidAutoTune = null;

        private double _output = 50;
        private int _windowSize = 5000;
        private long _windowStartTime;
        private double _kp = 2;
        private double _ki = 0.5;
        private double _kd = 2;
        private double _aTuneStep=50;
        private double _aTuneNoise=1;
        private double _aTuneStartValue=100;
        private int _aTuneLookBack = 20;
        private bool _tuning = true;
        private PIDController.PID_Mode _autoTuneModeRemember = PIDController.PID_Mode.AUTOMATIC;


        public TemperatureControlService(OutputHelper helper)
        {
            _outputHelper = helper;

            _thermistor = new Thermistor(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A0);
            _thermistor.VoltageReference = 3.3f;
            _thermistor.ResistanceReference = 1470000;

            _pid = new PIDController(PinManagement.currentTemperatureSensor, _output, PinManagement.setTemperature, _kp, _ki, _kd, PIDController.PID_Direction.DIRECT);

            _windowSize = (int)SystemSettings.MinutesBetweenReadings;

            _pid.SetOutputLimits(0, _windowSize);

            _pid.Mode = PIDController.PID_Mode.AUTOMATIC;

            _windowStartTime = DateTime.Now.Ticks;

            _pidAutoTune = new PIDAutoTune();

            if (_tuning)
            {
                _tuning = false;

                ChangeAutoTune();

                _tuning = true;
            }
        }

        private void ChangeAutoTune()
        {
            if (!_tuning)
            {
                //Set the output to the desired starting frequency.
                _output = _aTuneStartValue;
                _pidAutoTune.NoiseBand = _aTuneNoise;
                _pidAutoTune.OutputStep = _aTuneStep;
                _pidAutoTune.LookbackSeconds = _aTuneLookBack;

                AutoTuneHelper(true);
                
                _tuning = true;
            }
            else
            { 
                //cancel autotune
                _pidAutoTune.Cancel();

                _tuning = false;

                AutoTuneHelper(false);
            }
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

                    if (_tuning)
                    {
                        _pidAutoTune.Input = PinManagement.currentTemperatureSensor;

                        int val = (_pidAutoTune.Runtime());

                        if (val != 0)
                        {
                            _tuning = false;
                        }
                        
                        if (!_tuning)
                        { //we're done, set the tuning parameters
                            _kp = _pidAutoTune.Kp;
                            _ki = _pidAutoTune.Ki;
                            _kd = _pidAutoTune.Kd;
                            _pid.SetTunings(_kp, _ki, _kd);

                            AutoTuneHelper(false);
                        }

                        _output = _pidAutoTune.Output;
                    }
                    else
                    {
                        _pid.Input = PinManagement.currentTemperatureSensor;
                        _pid.SetPoint = PinManagement.setTemperature;

                        _pid.Compute();

                        _output = _pid.Output;
                    }

                    if (PinManagement.heaterEngaged)
                    {
                        if ((DateTime.Now.Ticks - _windowStartTime) > _windowSize)
                        {
                            _windowStartTime += _windowSize;
                        }

                        if (_output < (DateTime.Now.Ticks - _windowStartTime))
                        {
                            // turn the heating element on
                            PinManagement.heaterOnOffPort.Write(true);
                            // set our internal flag
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

        private void AutoTuneHelper(bool start)
        {
            if (start)
            {
                _autoTuneModeRemember = _pid.Mode;
            }
            else
            {
                _pid.Mode = _autoTuneModeRemember;
            }
        }
    }
}
