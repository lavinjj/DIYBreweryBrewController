using System;

namespace CodingSmackdown.Sensors
{
    public interface ITemperatureSensor : IDisposable
    {
        int MaximumTemperatureCapability { get; }

        int MinimumTemperatureCapability { get; }

        float GetTemperatureInC();

        float GetTemperatureInF();
    }
}