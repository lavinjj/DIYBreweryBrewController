using System;
using System.Threading;
using CodingSmackdown.Sensors;
using CodingSmackdown.Services.Interfaces;

namespace CodingSmackdown.Services
{
    public class MashProfileControlService : ServiceBase
    {
        private readonly IOutputHelper _outputHelper = null;
        private MashStep _currentStep = null;
        private bool _reachedStepTemperature = false;
        private bool _stepsComplete = false;
        private DateTime _stepStartTime = DateTime.MinValue;

        public MashProfileControlService(IOutputHelper helper)
        {
            _outputHelper = helper;
        }

        public void StartMashCycle()
        {
            _currentStep = null;
            _stepStartTime = DateTime.MinValue;
            _stepsComplete = false;
            _reachedStepTemperature = false;
        }

        protected override void Run()
        {
            while (true)
            {
                try
                {
                    if (PinManagement.heaterEngaged)
                    {
                        if ((PinManagement.mashSteps != null) && (PinManagement.mashSteps.Steps.Count > 0))
                        {
                            _currentStep = PinManagement.mashSteps.CurrentStep;

                            if ((_currentStep == null) && (!_stepsComplete))
                            {
                                _currentStep = PinManagement.mashSteps.First();
                                PinManagement.setTemperature = _currentStep.Temperature;
                                _stepStartTime = DateTime.Now;
                            }

                            DateTime _currentTime = DateTime.Now;
                            // calculate the difference between the set temperature and current temperature
                            float temperatureDifference = (float)System.Math.Abs(PinManagement.setTemperature - PinManagement.currentTemperatureSensor);
                            // are we at the step temperature
                            if (temperatureDifference < SystemSettings.TemperatureHeaterOffset)
                            {
                                if (_reachedStepTemperature)
                                {
                                    // check how long at the step temperature
                                    TimeSpan timeInterval = _currentTime.Subtract(_stepStartTime);

                                    _outputHelper.DisplayText("Step: " + _currentStep.StepNumber.ToString() + "Temp: " + _currentStep.Temperature.ToString("f2") + "|Time: " + timeInterval.Minutes.ToString());
                                    
                                    if (timeInterval.Minutes >= _currentStep.Time)
                                    {
                                        // pulse pezio for five seconds to indicate profile step complete
                                        PinManagement.buzzerSolidPort.Write(true);
                                        Thread.Sleep(5000);
                                        PinManagement.buzzerSolidPort.Write(false);
                                        // get the next step
                                        _currentStep = PinManagement.mashSteps.Next();
                                        // set the temp
                                        PinManagement.setTemperature = _currentStep.Temperature;
                                        // set the start time
                                        _stepStartTime = DateTime.Now;
                                        // update the flag
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
                                    PinManagement.currentMashStepStartTime = _stepStartTime;
                                    // update the flag
                                    _reachedStepTemperature = true;
                                }
                            }
                            else
                            {
                                if (!_reachedStepTemperature)
                                {
                                    // reset the minutes remaining time
                                    PinManagement.currentMashStepStartTime = _currentTime;
                                }
                                else
                                {
                                    // check how long at the step temperature
                                    TimeSpan timeInterval = _currentTime.Subtract(_stepStartTime);
                                    _outputHelper.DisplayText("Step: " + _currentStep.StepNumber.ToString() + "Temp: " + _currentStep.Temperature.ToString("f2") + "|Time: " + timeInterval.Minutes.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                // wait till next reading cycle
                Thread.Sleep(1000);
            }
        }
    }
}