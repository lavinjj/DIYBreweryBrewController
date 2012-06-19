using System;

namespace CodingSmackdown.Sensors
{
    public interface ITemperatureSensor : IDisposable
    {
        int MaximumTemperatureCapability { get; }

        int MinimumTemperatureCapability { get; }

        float ResistanceReference { get; set; }

        float VoltageReference { get; set; }

        float GetTemperatureInC();

        float GetTemperatureInF();
    }
}