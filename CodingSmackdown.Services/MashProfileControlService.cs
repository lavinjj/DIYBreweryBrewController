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
    public class MashProfileControlService : ServiceBase
    {
        private readonly OutputHelper _outputHelper = null;
        private MashStep _currentStep = null;
        private DateTime _stepStartTime = DateTime.MinValue;
        private bool _stepsComplete = false;
        private bool _reachedStepTemperature = false;

        public MashProfileControlService(OutputHelper helper)
        {
            _outputHelper = helper;
        }

        protected override void Run()
        {
            while (true)
            {
                try
                {
                    if (PinManagement.heaterEngaged)
                    {
                        if((PinManagement.mashSteps != null) && (PinManagement.mashSteps.Steps.Count > 0))
                        {
                            if ((_currentStep == null) && (!_stepsComplete))
                            {
                                _currentStep = PinManagement.mashSteps.First();
                                _stepStartTime = DateTime.Now;
                            }

                            DateTime _currentTime = DateTime.Now;
                            // calculate the difference between the set temperature and current temperature
                            float temperatureDifference = (float)System.Math.Abs(PinManagement.setTemperature - PinManagement.currentTemperatureSensor1);
                            // are we at the step temperature
                            if (temperatureDifference < SystemSettings.TemperatureHeaterOffset)
                            {
                                if (_reachedStepTemperature)
                                {
                                    // check how long at the step temperature
                                    TimeSpan timeInterval = _currentTime.Subtract(_stepStartTime);
                                    if (timeInterval.Minutes >= _currentStep.Time)
                                    {
                                        // pulse pezio for five seconds to indicate profile step complete
                                        PinManagement.buzzerPort.Write(true);
                                        Thread.Sleep(5000);
                                        PinManagement.buzzerPort.Write(false);
                                        // get the next step
                                        _currentStep = PinManagement.mashSteps.Next();
                                        // set the start time
                                        _stepStartTime = DateTime.Now;
                                        // udpate the flag
                                        _reachedStepTemperature = false;
                                        // no more steps turn off the heater
                                        if (_currentStep == null)
                                        {
                                            PinManagement.setTemperature = 0.0F;
                                            PinManagement.heaterEngaged = false;
                                            _stepsComplete = true;
                                        }
                                    }
                                }
                                else
                                {
                                    // we have reached the step temperature
                                    // update the start time
                                    _stepStartTime = DateTime.Now;
                                    // udpate the flag
                                    _reachedStepTemperature = true;
                                }
                            }
                            else
                            {
                                // we are not at the step temperature so set the step temperature
                                PinManagement.setTemperature = _currentStep.Temperature;
                                // update the start time
                                _stepStartTime = DateTime.Now;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                // wait til next reading cycle
                Thread.Sleep((int)SystemSettings.MinutesBetweenReadings);
            }
        }
    
        public void StartMashCycle()
        {
            _currentStep = null;
            _stepStartTime = DateTime.MinValue;
            _stepsComplete = false;
            _reachedStepTemperature = false;
        }
    }
}
