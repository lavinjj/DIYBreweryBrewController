using System;
using MicroLiquidCrystal;
using Microsoft.SPOT;

namespace CodingSmackdown.Services.Interfaces
{
    public interface IOutputHelper
    {
        Lcd DisplayController { get; set; }

        string HistoryFileName { get; set; }

        void DisplayText(string message);

        void UpdateTemeratureDisplay();

        void UpdateTemperatureLogFile();
    }
}