using System.Collections;

namespace CodingSmackdown.Services
{
    public partial class MashStep
    {
        public int StepNumber { get; set; }

        public float Temperature { get; set; }

        public int Time { get; set; }
    }

    public partial class MashSteps
    {
        private MashStep _currentStep;
        private int _index;
        private ArrayList _steps;

        public MashSteps()
        {
            _steps = new ArrayList();
        }

        public MashStep CurrentStep
        {
            get { return _currentStep; }
            set { _currentStep = value; }
        }

        public ArrayList Steps
        {
            get
            {
                return _steps;
            }

            set
            {
                _steps = value;
            }
        }

        public MashStep First()
        {
            _index = 0;
            MashStep result = null;

            if ((_steps != null) && (_steps.Count > 0))
            {
                result = (MashStep)_steps[_index];
                _currentStep = result;
            }
            else
            {
                _currentStep = null; ;
            }

            return result;
        }

        public MashStep Next()
        {
            _index++;
            MashStep result = null;

            if ((_steps != null) && (_index < _steps.Count))
            {
                result = (MashStep)_steps[_index];
                _currentStep = result;
            }
            else
            {
                _currentStep = null; ;
            }

            return result;
        }
    }
}