using System.Threading;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Hardware;

namespace CodingSmackdown.Sensors
{
    public class Thermistor : ITemperatureSensor
    {
        private const double TABS = -273.15;
        protected SecretLabs.NETMF.Hardware.AnalogInput sensor;

        private double[] a = {
			2.900538876296411e-005,
			4.503191323434938e-004,
			-3.288268667833969e-005,
			1.243331808293418e-006,
		};

        /// <summary>
        /// Create a new base instance of the temperature sensor.
        /// </summary>
        /// <param name="pin">Pin the sensor's vout is connected to.
        public Thermistor(Cpu.Pin pin)
        {
            // http://www.analog.com/en/temperature-sensing-and-thermal-management/digital-temperature-sensors/tmp36/products/product.html
            MaximumTemperatureCapability = 125;
            MinimumTemperatureCapability = -40;
            RequiredVoltage = 2.7f;

            sensor = new SecretLabs.NETMF.Hardware.AnalogInput(pin);
            sensor.SetRange(0, 3300);
        }

        public int MaximumTemperatureCapability { get; protected set; }

        public int MinimumTemperatureCapability { get; protected set; }

        public float RequiredVoltage { get; protected set; }

        public float ResistanceReference { get; set; }

        public float VoltageReference { get; set; }

        public void Dispose()
        {
            sensor.Dispose();
            sensor = null;
        }

        public float GetTemperatureInC()
        {
            double tempReading = 0;
            // read the sensor 100 times and add up the value
            // so we can get an average reading
            for (int i = 0; i < 100; i++)
            {
                tempReading += sensor.Read();
                Thread.Sleep(10);
            }
            // calculate the temperature
            double milliVolts = tempReading / 100000;

            double vPad = VoltageReference - milliVolts;
            // Use the following if your thermistor circuit is hooked up as follows
            // GND|-----|Thermistor|--|A0|--|/\/\Ref Resistance/\/\|--|+3.3V
            //double circuitCurrent = vPad / ResistanceReference;
            //
            //double thermResistance = milliVolts / circuitCurrent;
            // end changes

            // Use the following if your thermistor circuit is hooked up as follows
            // +3.3V|-----|Thermistor|--|A0|--|/\/\Ref Resistance/\/\|--|GND
            double circuitCurrent = milliVolts / ResistanceReference;

            double thermResistance = vPad / circuitCurrent;
            // end changes

            double tempCelsius = rtot(thermResistance);

            return (float)tempCelsius;
        }

        public float GetTemperatureInF()
        {
            return GetTemperatureInC() * 9 / 5 + 32;
        }

        private double poly(double x, int degree, double[] p)
        {
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

        /** Coefficients of Steinhart-Hart polynom. */
    }
}
