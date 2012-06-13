using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CodingSmackdown.Sensors
{
    public interface ITemperatureSensor : IDisposable
    {
        float GetTemperatureInC();
        float GetTemperatureInF();

        int MaximumTemperatureCapability { get; }
        int MinimumTemperatureCapability { get; }
    }
}
