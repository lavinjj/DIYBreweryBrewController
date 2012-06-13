using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CodingSmackdown.Sensors
{
    public class TMP36 : ITemperatureSensor
    {
        protected SecretLabs.NETMF.Hardware.AnalogInput sensor; 

        /// <summary> 
        /// Create a new base instance of the temperature sensor. 
        /// </summary> 
        /// <param name="pin">Pin the sensor's vout is connected to. 
        public TMP36(Cpu.Pin pin)
        {
            // http://www.analog.com/en/temperature-sensing-and-thermal-management/digital-temperature-sensors/tmp36/products/product.html
            MaximumTemperatureCapability = 125;
            MinimumTemperatureCapability = -40;
            RequiredVoltage = 2.7f;

            sensor = new SecretLabs.NETMF.Hardware.AnalogInput(pin);
            sensor.SetRange(0, 3300);
        }

        public float GetTemperatureInC()
        {
            // gain = 10 mV/Deg C 
            double totalReading = 0;

            for (int i = 0; i < 100; i++)
            {
                totalReading += sensor.Read();
                Thread.Sleep(10);
            }

            double milliVolts = totalReading / 100f;

            return (float)(milliVolts - 500) / 10f;
        }

        public float GetTemperatureInF()
        {
            return GetTemperatureInC() * 9 / 5 + 32;
        }

        public int MaximumTemperatureCapability { get; protected set; }

        public int MinimumTemperatureCapability { get; protected set; }

        public float RequiredVoltage { get; protected set; }

        public void Dispose()
        {
            sensor.Dispose();
            sensor = null;
        }
    }
}