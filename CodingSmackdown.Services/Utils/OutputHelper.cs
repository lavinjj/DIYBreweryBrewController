using System;
using System.IO;
using System.Text;
using CodingSmackdown.Services.Interfaces;
using CodingSmackdown.Drivers;

namespace CodingSmackdown.Services
{
    public class OutputHelper : IOutputHelper
    {
        private static object s_IncLock = new object();
        private SPI_VFDisplay _displayController = null;
        private string _historyFileName = String.Empty;

        public SPI_VFDisplay DisplayController
        {
            get { return _displayController; }
            set { _displayController = value; }
        }

        public string HistoryFileName
        {
            get { return _historyFileName; }
            set { _historyFileName = value; }
        }

        public void DisplayText(string message)
        {
            lock (s_IncLock)
            {
                _displayController.clear();
                _displayController.home();
                // handle two lines being sent to the lcd display
                if (message.IndexOf('|') > 0)
                {
                    string[] output = message.Split('|');
                    _displayController.print(output[0]);
                    _displayController.setCursor(0, 1);
                    _displayController.print(output[1]);
                }
                else
                {
                    _displayController.print(message);
                }
            }
        }

        public void UpdateTemeratureDisplay()
        {
            lock (s_IncLock)
            {
                _displayController.clear();
                _displayController.home();

                StringBuilder displayString = new StringBuilder();
                displayString.Append("Set Temp: ");
                displayString.Append(PinManagement.setTemperature.ToString("f2"));

                _displayController.print(displayString.ToString());

                displayString = new StringBuilder();
                displayString.Append("Cnt Temp: ");
                displayString.Append(PinManagement.currentTemperatureSensor.ToString("f2"));
                if (PinManagement.isHeating)
                {
                    displayString.Append(" *");
                }
                else
                {
                    displayString.Append("  ");
                }
                if (PinManagement.autoTuning)
                {
                    displayString.Append("A");
                }
                _displayController.setCursor(0, 1);
                _displayController.print(displayString.ToString());
            }
        }

        public void UpdateTemperatureLogFile()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@"\SD\" + _historyFileName, true))
                {
                    DateTime recordTime = DateTime.Now;

                    file.Write(recordTime.ToString());

                    file.Write("," + PinManagement.temperatureCelsiusSensor.ToString("f4"));

                    file.Write("," + PinManagement.currentTemperatureSensor.ToString("f4"));

                    file.Write("," + PinManagement.setTemperature.ToString("f4"));

                    if (PinManagement.isHeating)
                    {
                        file.WriteLine(",1");
                    }
                    else
                    {
                        file.WriteLine(",0");
                    }

                    file.Flush();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}