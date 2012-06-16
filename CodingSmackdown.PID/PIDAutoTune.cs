/**********************************************************************************************
 * .NET MF PID AutoTune Library - Version 0.0.0
 * by Jim Lavin 06/12/2012
 *
 * This library was proted from the
 * Arduino PID AutoTune Library - Version 0.0.0
 * by Brett Beauregard <br3ttb@gmail.com> brettbeauregard.com
 *
 * which was ported from the AutotunerPID Toolkit by William Spinelli
 * (http://www.mathworks.com/matlabcentral/fileexchange/4652)
 * Copyright (c) 2004
 *
 * This Library is licensed under the BSD License:
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in
 *       the documentation and/or other materials provided with the distribution
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 **********************************************************************************************/

using System;

namespace CodingSmackdown.PID
{
    public class PIDAutoTune
    {
        private double _absMax;
        private double _absMin;
        private bool _isMax;
        private bool _isMin;
        private bool _justchanged;
        private double _ku;
        private double[] _lastInputs;
        private long _lastTime;
        private int _lookBack;
        private double _outputStart;
        private long _peak1;
        private long _peak2;
        private int _peakCount;
        private double[] _peaks;
        private int _peakType;
        private double _pu;
        private bool _running;
        private int _sampleTime;
        private double _setpoint;

        public PIDAutoTune()
        {
            _lastInputs = new double[100];
            _peaks = new double[10];
        }

        public enum ControllerType
        {
            PI = 0,
            PID = 1
        }

        /// <summary>
        /// Determies if the tuning parameters returned will be PI (D=0)
        /// or PID.  (0=PI, 1=PID)
        /// </summary>
        public ControllerType ControlType { get; set; }

        public double Input { get; set; }

        //Kd = Kc * Td
        public double Kd
        {
            get { return ControlType == ControllerType.PID ? 0.075 * _ku * _pu : 0; }
        }

        public double Ki
        {
            // Ki = Kc/Ti
            get { return ControlType == ControllerType.PID ? 1.2 * _ku / _pu : 0.48 * _ku / _pu; }
        }

        public double Kp
        {
            get { return ControlType == ControllerType.PID ? 0.6 * _ku : 0.4 * _ku; }
        }

        /// <summary>
        /// how far back are we looking to identify peaks
        /// </summary>
        public int LookbackSeconds
        {
            get
            {
                return _lookBack * _sampleTime / 1000;
            }

            set
            {
                if (value < 1) value = 1;

                if (value < 25)
                {
                    _lookBack = value * 4;
                    _sampleTime = 250;
                }
                else
                {
                    _lookBack = 100;
                    _sampleTime = value * 10;
                }
            }
        }

        /// <summary>
        /// the autotune will ignore signal chatter smaller than this value
        /// this should be acurately set
        /// </summary>
        public double NoiseBand { get; set; }

        public double Output { get; set; }

        /// <summary>
        /// how far above and below the starting value will the output step?
        /// </summary>
        /// <param name="?"></param>
        public double OutputStep { get; set; }

        /// <summary>
        /// Stops the AutoTune
        /// </summary>
        public void Cancel()
        {
            _running = false;
        }

        /// <summary>
        /// Similar to the PID Compue function, returns non 0 when done
        /// </summary>
        /// <returns></returns>
        public int Runtime()
        {
            if (_peakCount > 9 && _running)
            {
                _running = false;
                FinishUp();
                return 1;
            }

            long now = DateTime.Now.Ticks;

            if ((now - _lastTime) < _sampleTime)
            {
                return 0;
            }

            _lastTime = now;

            double refVal = Input;

            if (!_running)
            {
                //initialize working variables the first time around
                _peakType = 0;
                _peakCount = 0;
                _justchanged = false;
                _absMax = refVal;
                _absMin = refVal;
                _setpoint = refVal;
                _running = true;
                _outputStart = Output;
                Output = _outputStart + OutputStep;
            }
            else
            {
                if (refVal > _absMax)
                {
                    _absMax = refVal;
                }

                if (refVal < _absMin)
                {
                    _absMin = refVal;
                }
            }

            //oscillate the output based on the input's relation to the setpoint
            if (refVal > _setpoint + NoiseBand)
            {
                Output = _outputStart - OutputStep;
            }
            else if (refVal < _setpoint - NoiseBand)
            {
                Output = _outputStart + OutputStep;
            }

            _isMax = true;
            _isMin = true;

            //id peaks
            for (int i = _lookBack - 1; i >= 0; i--)
            {
                double val = _lastInputs[i];

                if (_isMax)
                {
                    _isMax = refVal > val;
                }

                if (_isMin)
                {
                    _isMin = refVal < val;
                }

                _lastInputs[i + 1] = _lastInputs[i];
            }

            _lastInputs[0] = refVal;

            if (_lookBack < 9)
            {
                //we don't want to trust the maxes or mins until the inputs array has been filled
                return 0;
            }

            if (_isMax)
            {
                if (_peakType == 0)
                {
                    _peakType = 1;
                }

                if (_peakType == -1)
                {
                    _peakType = 1;
                    _justchanged = true;
                    _peak2 = _peak1;
                }

                _peak1 = now;

                _peaks[_peakCount] = refVal;
            }
            else if (_isMin)
            {
                if (_peakType == 0)
                {
                    _peakType = -1;
                }

                if (_peakType == 1)
                {
                    _peakType = -1;
                    _peakCount++;
                    _justchanged = true;
                }

                if (_peakCount < 10) _peaks[_peakCount] = refVal;
            }

            if (_justchanged && _peakCount > 2)
            {
                //we've transitioned.  check if we can autotune based on the last peaks
                double avgSeparation = (System.Math.Abs(_peaks[_peakCount - 1] - _peaks[_peakCount - 2]) + System.Math.Abs(_peaks[_peakCount - 2] - _peaks[_peakCount - 3])) / 2;

                if (avgSeparation < 0.05 * (_absMax - _absMin))
                {
                    FinishUp();
                    _running = false;
                    return 1;
                }
            }

            _justchanged = false;

            return 0;
        }

        private void FinishUp()
        {
            Output = _outputStart;
            //we can generate tuning parameters!
            _ku = 4 * (2 * OutputStep) / ((_absMax - _absMin) * 3.14159);
            _pu = (double)(_peak1 - _peak2) / 1000;
        }
    }
}