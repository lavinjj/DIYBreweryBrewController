using System;
using System.Threading;
using CodingSmackdown.PID;
using CodingSmackdown.Sensors;
using CodingSmackdown.Services.Interfaces;

namespace CodingSmackdown.Services
{
    public class TemperatureControlService : ServiceBase
    {
        private readonly IOutputHelper _outputHelper = null;
        private int _aTuneLookBack = 20;
        private double _aTuneNoise = 1;
        private double _aTuneStartValue = 100;
        private double _aTuneStep = 50;
        private PIDController.PID_Mode _autoTuneModeRemember = PIDController.PID_Mode.AUTOMATIC;
        private double _kd = 0;
        private double _ki = 461.53;
        private double _kp = 450;
        private double _output = 0;
        private PIDController _pid = null;
        private PIDAutoTune _pidAutoTune = null;
        private ITemperatureSensor _thermistor = null;
        private bool _tuning = true;
        private int _windowSize = 1000;
        private long _windowStartTime;

      public TemperatureControlService(IOutputHelper helper, ITemperatureSensor sensor, Settings systemSettings)
        {
            SystemSettings = systemSettings;

            _outputHelper = helper;

            _thermistor = sensor;

            _pid = new PIDController(PinManagement.currentTemperatureSensor, _output, PinManagement.setTemperature, SystemSettings.PIDKp, SystemSettings.PIDKi, SystemSettings.PIDKd, PIDController.PID_Direction.DIRECT);

            _pid.SetOutputLimits(0, 100);

            _pid.SampleTime = _windowSize;

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
                        _outputHelper.DisplayText("PID|Auto Tuning");

                        _pidAutoTune.Input = PinManagement.currentTemperatureSensor;

                        int val = (_pidAutoTune.Runtime());

                        if (val != 0)
                        {
                            _tuning = false;
                        }

                        if (!_tuning)
                        { 
                            //we're done, set the tuning parameters
                            _kp = _pidAutoTune.Kp;
                            SystemSettings.PIDKp = (float)_pidAutoTune.Kp; 

                            _ki = _pidAutoTune.Ki;
                            SystemSettings.PIDKi = (float)_pidAutoTune.Ki; 
                            
                            _kd = _pidAutoTune.Kd;
                            SystemSettings.PIDKd = (float)_pidAutoTune.Kd;

                            SystemSettings.saveSettings();

                            _pid.SetTunings(_kp, _ki, _kd);

                            _outputHelper.DisplayText("kp: " + _kp.ToString("f3"));
                            Thread.Sleep(5000);
                            _outputHelper.DisplayText("ki: " + _ki.ToString("f3"));
                            Thread.Sleep(5000);
                            _outputHelper.DisplayText("kd: " + _kd.ToString("f3"));
                            Thread.Sleep(5000);

                            AutoTuneHelper(false);
                        }

                        _output = _pidAutoTune.Output;
                        PinManagement.currentPIDOuput = (float)_pidAutoTune.Output;
                    }
                    else
                    {
                        _pid.Input = PinManagement.currentTemperatureSensor;
                        _pid.SetPoint = PinManagement.setTemperature;

                        _pid.Compute();

                        _output = _pid.Output;
                        PinManagement.currentPIDOuput = (float)_pid.Output;
                    }

                    if (PinManagement.heaterEngaged)
                    {
                        if ((DateTime.Now.Ticks - _windowStartTime) > _windowSize)
                        {
                            _windowStartTime += _windowSize;
                        }

                        if (_output > 0)
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
                if (System.Math.Abs(_output) > 0)
                {
                    Thread.Sleep((int)(_windowSize * System.Math.Abs(_output)));
                }
                else
                {
                    Thread.Sleep((int)(SystemSettings.MinutesBetweenReadings));
                }
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
    }
}
