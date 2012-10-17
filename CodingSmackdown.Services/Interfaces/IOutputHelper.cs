using System;
using Microsoft.SPOT;
using CodingSmackdown.Drivers;

namespace CodingSmackdown.Services.Interfaces
{
    public interface IOutputHelper
    {
        SPI_VFDisplay DisplayController { get; set; }

        string HistoryFileName { get; set; }

        void DisplayText(string message);

        void UpdateTemeratureDisplay();

        void UpdateTemperatureLogFile();
    }
}