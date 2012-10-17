using System;
using System.Text;
using System.Threading;
using CodingSmackdown.Sensors;
using CodingSmackdown.Services;
using CodingSmackdown.Services.Interfaces;
using Microsoft.SPOT.Hardware;
using NeonMika.Webserver;
using NeonMika.Webserver.Responses;
using CodingSmackdown.Drivers;

namespace CodingSmackdown.BrewController
{
    public class Program
    {
        private static IOutputHelper _displayHelper = null;
        private static ITemperatureSensor _thermistor = null;
        private static DateTime allStopButtonLastPushed = DateTime.MinValue;
        private static DateTime engageHeaterButtonLastPushed = DateTime.MinValue;
        private static Thread mainThread;
        private static DateTime setTemperatureUpButtonLastPushed = DateTime.MinValue;
        private static DateTime setTemperatureDownButtonLastPushed = DateTime.MinValue;

        public static void Main()
        {
            mainThread = Thread.CurrentThread;

            // create the LCD interface
            var lcd = new SPI_VFDisplay(SPI.SPI_module.SPI1, SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D10, SPI_VFDisplay.Brightness.VFD_BRIGHTNESS25);

            // set up the LCD's number of columns and rows:
            lcd.begin(20, 2, SPI_VFDisplay.Brightness.VFD_BRIGHTNESS25);
            // set up the LCD's number of columns and rows: 
            lcd.clear();
            // create the output helper and attach tot he LCD Display
            _displayHelper = new OutputHelper();
            _displayHelper.DisplayController = lcd;

            // Print a message to the LCD.
            _displayHelper.DisplayText("The DIY Brewery|Brew Controller");

            Thread.Sleep(5000);

            _displayHelper.DisplayText("Loading|Settings");

            Settings settings = new Settings();
            settings.loadSettings();

            _displayHelper.HistoryFileName = settings.HistoryFilename;

            UpdateSettingsMethod.UpdateNetworkConfiguration(settings);

            _displayHelper.DisplayText("Network|Started");

            _displayHelper.DisplayText(UpdateSettingsMethod.GetNetworkIP(settings));

            Thread.Sleep(5000);

            Server WebServer = new Server(80);
            WebServer.AddResponse(new JSONResponse("temperature", new JSONResponseCheck(GetTemperatureMethod.GetTemperature)));
            WebServer.AddResponse(new JSONResponse("settings", new JSONResponseCheck(GetSettingsMethod.GetSettings)));
            WebServer.AddResponse(new JSONResponse("updateSettings", new JSONResponseCheck(UpdateSettingsMethod.UpdateSettings)));
            WebServer.AddResponse(new JSONResponse("updateMashProfile", new JSONResponseCheck(UpdateMashProfileMethod.UpdateMashProfile)));

            _displayHelper.DisplayText("Web Server|Started");

            Thread.Sleep(5000);

            NTPTimeService timeService = new NTPTimeService();
            timeService.SystemSettings = settings;
            timeService.Start();

            _displayHelper.DisplayText("Internet Time|Service Started");

            Thread.Sleep(5000);

            MashProfileControlService mashProfileService = new MashProfileControlService(_displayHelper);
            mashProfileService.SystemSettings = settings;
            mashProfileService.Start();

            _displayHelper.DisplayText("Mash Profile|Service Started");

            Thread.Sleep(5000);

            _thermistor = new Thermistor(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A0);
            _thermistor.ResistanceReference = settings.PadResistance;
            _thermistor.VoltageReference = settings.VoltageReference;

            TemperatureControlService tempLogger = new TemperatureControlService(_displayHelper, _thermistor, settings);
            tempLogger.Start();

            _displayHelper.DisplayText("Temp Monitor|Started");

            Thread.Sleep(5000);

            PinManagement.engageHeaterButton.OnInterrupt += new NativeEventHandler(EngageHeaterButton_OnInterrupt);
            PinManagement.setTemperatureUpButton.OnInterrupt += new NativeEventHandler(TemperatureSetUp_OnInterrupt);
            PinManagement.setTemperatureUpDown.OnInterrupt += new NativeEventHandler(TemperatureSetDown_OnInterrupt);
            PinManagement.allStopButton.OnInterrupt += new NativeEventHandler(AllStop_OnInterrupt);

            _displayHelper.DisplayText("Inputs|Active");

            mainThread.Suspend();
        }

      public static void EngageHeaterButton_OnInterrupt(uint data1, uint data2, DateTime time) 
        {
            if (engageHeaterButtonLastPushed.AddMilliseconds(200) > time)
                return;
            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                PinManagement.heaterEngaged = true;
            }
            _displayHelper.DisplayText("Heater|Engaged");
            engageHeaterButtonLastPushed = time;
        }

        public static void TemperatureSetUp_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (setTemperatureUpButtonLastPushed.AddMilliseconds(200) > time)
                return;
            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                PinManagement.setTemperature++;
                PinManagement.alarmSounded = false;
                if (PinManagement.setTemperature >= 300.0)
                {
                    PinManagement.setTemperature = 300.0F;
                }
            }

            StringBuilder message = new StringBuilder();
            message.Append("Set Temp|");
            message.Append(PinManagement.setTemperature.ToString("f2"));
            _displayHelper.DisplayText(message.ToString());

            setTemperatureUpButtonLastPushed = time;
        }

        public static void TemperatureSetDown_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (setTemperatureDownButtonLastPushed.AddMilliseconds(200) > time)
                return;

            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                PinManagement.setTemperature--;
                PinManagement.alarmSounded = false;
                 if (PinManagement.setTemperature <= 0.0)
                {
                    PinManagement.setTemperature = 0.0F;
                }
            }

            StringBuilder message = new StringBuilder();
            message.Append("Set Temp|");
            message.Append(PinManagement.setTemperature.ToString("f2"));
            _displayHelper.DisplayText(message.ToString());

            setTemperatureDownButtonLastPushed = time;
        }

        public static void AllStop_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (allStopButtonLastPushed.AddMilliseconds(200) > time)
                return;
            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                // PinManagement.heaterOnOffPort.SetDutyCycle(0);
                PinManagement.heaterOnOffPort.Write(false);
                PinManagement.heaterEngaged = false;
            }
            _displayHelper.DisplayText("Heater|Dis-Engaged");
            allStopButtonLastPushed = time;
        }
    }
}
