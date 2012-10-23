/**********************************************************************************************
 * Converted to .NET MicroFramework by Jim Lavin 06/12/2012
 *
 * Based on Arduino PID Library - Version 1.0.1
 * by Brett Beauregard <br3ttb@gmail.com> brettbeauregard.com
 *
 * This Library is licensed under a GPLv3 License
 **********************************************************************************************/

using System;

namespace CodingSmackdown.PID
{
    public class PIDController
    {
        private PIDController.PID_Direction _direction;
        private bool _inAutoMode;
        private double _iTerm;
        private double _lastInput;
        private long _lastTime;
        private PIDController.PID_Mode _mode;
        private double _outMax;
        private double _outMin;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="setpoint"></param>
        /// <param name="kp"></param>
        /// <param name="ki"></param>
        /// <param name="kd"></param>
        /// <param name="controllerDirection"></param>
        public PIDController(double input, double output, double setpoint, double kp, double ki, double kd, PIDController.PID_Direction controllerDirection)
        {
            SetOutputLimits(0, 255);

            SampleTime = 1000;

            Direction = controllerDirection;

            SetTunings(kp, ki, kd);

            _lastTime = DateTime.Now.Ticks - (SampleTime * (long)output);

            _inAutoMode = false;

            Input = input;

            Output = output;

            SetPoint = setpoint;
        }

        public enum PID_Direction
        {
            DIRECT = 0,
            REVERSE = 1
        }

        public enum PID_Mode
        {
            MANUAL = 0,
            AUTOMATIC = 1
        }

        /// <summary>
        /// Sets the Direction, or "Action" of the controller. DIRECT
        /// means the output will increase when error is positive. REVERSE
        /// means the opposite.  it's very unlikely that this will be needed
        /// once it is set in the constructor.
        /// </summary>
        public PIDController.PID_Direction Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                if (_inAutoMode && value != _direction)
                {
                    Kp = (0 - Kp);
                    Ki = (0 - Ki);
                    Kd = (0 - Kd);
                }

                _direction = value;
            }
        }

        /// <summary>
        /// Input value from the current cycle
        /// </summary>
        public double Input { get; set; }

        /// <summary>
        /// (D)erivative Tuning Parameter
        /// </summary>
        public double Kd { get; set; }

        /// <summary>
        /// (I)ntegral Tuning Parameter
        /// </summary>
        public double Ki { get; set; }

        /// <summary>
        /// (P)roportional Tuning Parameter
        /// </summary>
        public double Kp { get; set; }

        /// <summary>
        /// Allows the controller Mode to be set to manual (0) or Automatic (non-zero)
        /// when the transition from manual to auto occurs, the controller is
        /// automatically initialized
        /// </summary>
        public PIDController.PID_Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                bool newAuto = (value == PID_Mode.AUTOMATIC);

                if (newAuto == !_inAutoMode)
                {
                    // we just went from manual to auto
                    Initialize();
                }

                _inAutoMode = newAuto;

                _mode = value;
            }
        }

        /// <summary>
        /// Output value for the current cycle
        /// </summary>
        public double Output { get; set; }

        /// <summary>
        /// Sets the frequency, in Milliseconds, with which
        /// the PID calculation is performed.  default is 1000
        /// </summary>
        public int SampleTime { get; set; }

        /// <summary>
        /// The value you are trying to reach
        /// </summary>
        public double SetPoint { get; set; }

        /// <summary>
        /// This, as they say, is where the magic happens.  this function should be called
        /// every time "void loop()" executes.  the function will decide for itself whether a new
        /// pid Output needs to be computed
        /// </summary>
        public void Compute()
        {
            if (!_inAutoMode) return;

            long now = DateTime.Now.Ticks;
            long timeChange = (now - _lastTime);

            if (timeChange >= (SampleTime * Output))
            {
                /*Compute all the working error variables*/
                double input = Input;

                double error = SetPoint - input;

                _iTerm += (Ki * error);

                if (_iTerm > _outMax)
                {
                    _iTerm = _outMax;
                }
                else if (_iTerm < _outMin)
                {
                    _iTerm = _outMin;
                }

                double dInput = (input - _lastInput);

                /*Compute PID Output*/
                double output = Kp * error + _iTerm - Kd * dInput;

                if (output > _outMax)
                {
                    output = _outMax;
                }
                else if (output < _outMin)
                {
                    output = _outMin;
                }

                Output = output;

                /*Remember some variables for next time*/
                _lastInput = input;
                _lastTime = now;
            }
        }

        /// <summary>
        /// This function will be used far more often than SetInputLimits.  while
        /// the input to the controller will generally be in the 0-1023 range (which is
        /// the default already,)  the output will be a little different.  maybe they'll
        /// be doing a time window and will need 0-8000 or something.  or maybe they'll
        /// want to clamp it from 0-125.  who knows.  at any rate, that can all be done
        /// here.
        /// </summary>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        public void SetOutputLimits(double Min, double Max)
        {
            if (Min >= Max) return;

            _outMin = Min;
            _outMax = Max;

            if (_inAutoMode)
            {
                if (Output > _outMax)
                {
                    Output = _outMax;
                }
                else if (Output < _outMin)
                {
                    Output = _outMin;
                }

                if (_iTerm > _outMax)
                {
                    _iTerm = _outMax;
                }
                else if (_iTerm < _outMin)
                {
                    _iTerm = _outMin;
                }
            }
        }

        /// <summary>
        /// This function allows the controller's dynamic performance to be adjusted.
        /// it's called automatically from the constructor, but tunings can also
        /// be adjusted on the fly during normal operation
        /// </summary>
        /// <param name="kp">(P)roportional Tuning Parameter</param>
        /// <param name="ki">(I)ntegral Tuning Parameter</param>
        /// <param name="kd">(D)erivative Tuning Parameter</param>
        public void SetTunings(double kp, double ki, double kd)
        {
            if (kp < 0 || ki < 0 || kd < 0) return;

            double SampleTimeInSec = ((double)SampleTime) / 1000;
            Kp = kp;
            Ki = ki * SampleTimeInSec;
            Kd = kd / SampleTimeInSec;

            if (Direction == PIDController.PID_Direction.REVERSE)
            {
                kp = (0 - kp);
                ki = (0 - ki);
                kd = (0 - kd);
            }
        }

        /// <summary>
        /// does all the things that need to happen to ensure a bumpless transfer
        /// from manual to automatic mode.
        /// </summary>
        private void Initialize()
        {
            _iTerm = Output;

            _lastInput = Input;

            if (_iTerm > _outMax)
            {
                _iTerm = _outMax;
            }
            else if (_iTerm < _outMin)
            {
                _iTerm = _outMin;
            }
        }
    }
}