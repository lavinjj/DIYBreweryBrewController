using System;
using System.IO;
using System.Threading;
using System.Text;
using Microsoft.SPOT;
using MicroLiquidCrystal;

namespace CodingSmackdown.Services
{
    public class OutputHelper
    {
        private Lcd _displayController = null;
        private static object s_IncLock = new object();
        private string _historyFileName = String.Empty;

        public Lcd DisplayController
        {
            get { return _displayController; }
            set { _displayController = value; }
        }

        public string HistoryFileName
        {
            get { return _historyFileName; }
            set { _historyFileName = value;  }
        }

        public void UpdateTemperatureLogFile()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@"\SD\" + _historyFileName, true))
                {
                    DateTime recordTime = DateTime.Now;

                    file.Write(recordTime.ToString());

                    file.Write("," + PinManagement.milliVoltsSensor1.ToString("f4"));

                    file.Write("," + PinManagement.resistanceSensor1.ToString("f4"));

                    file.Write("," + PinManagement.temperatureCelciusSensor1.ToString("f4"));

                    file.Write("," + PinManagement.currentTemperatureSensor1.ToString("f4"));

                    file.Write("," + PinManagement.setTemperature.ToString("f4"));

                    file.Write("," + PinManagement.milliVoltsSensor2.ToString("f4"));

                    file.Write("," + PinManagement.temperatureCelciusSensor2.ToString("f4"));

                    file.Write("," + PinManagement.currentTemperatureSensor2.ToString("f4"));

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

        public void UpdateTemeratureDisplay()
        {
            lock (s_IncLock)
            {
                _displayController.Clear();
                _displayController.Home();

                StringBuilder displayString = new StringBuilder();
                displayString.Append("Set Temp: ");
                displayString.Append(PinManagement.setTemperature.ToString("f2"));

                _displayController.Write(displayString.ToString());

                displayString = new StringBuilder();
                displayString.Append("Cnt Temp: ");
                displayString.Append(PinManagement.currentTemperatureSensor1.ToString("f2"));
                _displayController.SetCursorPosition(0, 1);
                _displayController.Write(displayString.ToString());
            }
        }

        public void DisplaySensorTemperatures()
        {
            lock (s_IncLock)
            {
                _displayController.Clear();
                _displayController.Home();

                StringBuilder displayString = new StringBuilder();
                displayString.Append("Sensor 1: ");
                displayString.Append(PinManagement.currentTemperatureSensor1.ToString("f2"));

                _displayController.Write(displayString.ToString());

                displayString = new StringBuilder();
                displayString.Append("Sensor 2: ");
                displayString.Append(PinManagement.currentTemperatureSensor2.ToString("f2"));
                _displayController.SetCursorPosition(0, 1);
                _displayController.Write(displayString.ToString());
            }
        }

        public void DisplayText(string message)
        {
            lock (s_IncLock)
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
}
