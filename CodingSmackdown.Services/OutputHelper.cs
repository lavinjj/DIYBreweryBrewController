using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using MicroLiquidCrystal;

namespace CodingSmackdown.Services
{
    public class OutputHelper
    {
        private Lcd _displayController = null;

        public Lcd DisplayController
        {
            get { return _displayController; }
            set { _displayController = value; }
        }

        public void UpdateTemperatureLogFile()
        {
            try
            {
                Settings settings = new Settings();
                settings.loadSettings();

                using (StreamWriter file = new StreamWriter(@"\SD\" + settings.HistoryFilename, true))
                {
                    DateTime recordTime = DateTime.Now;

                    file.Write(recordTime.ToString());

                    file.Write("," + PinManagement.milliVolts.ToString("f4"));

                    file.Write("," + PinManagement.temperatureCelcius.ToString("f4"));

                    file.Write("," + PinManagement.currentTemperature.ToString("f4"));

                    file.Write("," + PinManagement.setTemperature.ToString("f4"));

                    if (PinManagement.isHeating)
                    {
                        file.WriteLine(",1");
                    }
                    else
                    {
                        file.WriteLine(",0");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void UpdateTemeratureDisplay()
        {
            _displayController.Clear();
            _displayController.Home();

            string displayString = "Set Temp: ";
            displayString += PinManagement.setTemperature.ToString("f2");

            _displayController.Write(displayString);

            displayString = "Cnt Temp: ";
            displayString += PinManagement.currentTemperature.ToString("f2");
            if (PinManagement.isHeating)
            {
                displayString += " Heat On";
            }
            else
            {
                displayString += " Heat Off";
            }

            _displayController.SetCursorPosition(0, 1);
            _displayController.Write(displayString);
        }

        public void DisplayText(string message)
        {
            _displayController.Clear();
            _displayController.Home();
            // handle two lines being sent to the lcd display
            if (message.IndexOf('|') > 0)
            {
                string[] output = message.Split('|');
                _displayController.Write(output[0]);
                _displayController.SetCursorPosition(0, 1);
                _displayController.Write(output[1]);
            }
            else
            {
                _displayController.Write(message);
            }
        }
    }
}
