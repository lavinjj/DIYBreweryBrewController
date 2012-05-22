using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Collections;
using NeonMika.NETMF;
using NeonMika.Webserver;
using NeonMika.Webserver.EventArgs;
using NeonMika.Webserver.Responses;
using CodingSmackdown.Services;
using MicroLiquidCrystal;

namespace CodingSmackdown.TemperatureController
{
    public class Program
    {
        private static Thread mainThread;
        private static OutputHelper _displayHelper = null;

        public static void Main()
        {
            mainThread = Thread.CurrentThread;

            // create the transfer provider
            var lcdProvider = new Shifter74Hc595LcdTransferProvider(SPI_Devices.SPI1, SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D10);

            // create the LCD interface
            var lcd = new Lcd(lcdProvider);

            // set up the LCD's number of columns and rows: 
            lcd.Begin(16, 2);

            // Print a message to the LCD.
            lcd.Write("DIY Brewery     Temp Controller");

            _displayHelper = new OutputHelper();
            _displayHelper.DisplayController = lcd;

            _displayHelper.DisplayText("Thread Started");

            _displayHelper.DisplayText("Display Init");

            Settings settings = new Settings();
            settings.loadSettings();

            UpdateSettingsMethod.UpdateNetworkConfiguration(settings);

            _displayHelper.DisplayText("Network Started");

            Server WebServer = new Server(80);
            WebServer.AddResponse(new JSONResponse("temperature", new JSONResponseCheck(GetTemperatureMethod.GetTemperature)));
            WebServer.AddResponse(new JSONResponse("settings", new JSONResponseCheck(GetSettingsMethod.GetSettings)));
            WebServer.AddResponse(new JSONResponse("updateSettings", new JSONResponseCheck(UpdateSettingsMethod.UpdateSettings)));

            _displayHelper.DisplayText("Web Started");

            NTPTimeService timeService = new NTPTimeService();
            timeService.SystemSettings = settings;
            timeService.Start();

            _displayHelper.DisplayText("NTP Started");

            Thread.Sleep(5000);

            NameService nameService = new NameService();
            nameService.AddName(settings.NetBiosName, NameService.NameType.Unique, NameService.MsSuffix.Default);

            _displayHelper.DisplayText("NETBIOS Started");

            Thread.Sleep(5000);

            TemperatureControlService tempLogger = new TemperatureControlService(_displayHelper);
            tempLogger.SystemSettings = settings;
            tempLogger.Start();

            _displayHelper.DisplayText("Temp Started");

            PinManagement.engageHeaterButton.OnInterrupt += new NativeEventHandler(EngageHeaterButton_OnInterrupt);
            PinManagement.setTemperatureUpButton.OnInterrupt += new NativeEventHandler(TemperatureSetUp_OnInterrupt);
            PinManagement.setTemperatureUpDown.OnInterrupt += new NativeEventHandler(TemperatureSetDown_OnInterrupt);
            PinManagement.allStopButton.OnInterrupt += new NativeEventHandler(AllStop_OnInterrupt);

            _displayHelper.DisplayText("Input Started");

            mainThread.Suspend();
        }

        public static void EngageHeaterButton_OnInterrupt(uint data1, uint data2, DateTime time) 
        {
            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                PinManagement.heaterEngaged = true;
            }
            _displayHelper.UpdateTemeratureDisplay();
        }

        public static void TemperatureSetUp_OnInterrupt(uint data1, uint data2, DateTime time)
        {
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
            _displayHelper.UpdateTemeratureDisplay();
        }

        public static void TemperatureSetDown_OnInterrupt(uint data1, uint data2, DateTime time)
        {
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
            _displayHelper.UpdateTemeratureDisplay();
        }

        public static void AllStop_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            // button press state received in data2
            // 0 = open, 1 = pressed
            if (data2 == 1)
            {
                PinManagement.heaterOnOffPort.Write(false);
                PinManagement.heaterEngaged = false;
            }
            _displayHelper.UpdateTemeratureDisplay();
        }
    }
}
