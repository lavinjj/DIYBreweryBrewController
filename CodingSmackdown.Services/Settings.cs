using System;
using Microsoft.SPOT;
using System.IO;

namespace CodingSmackdown.Services
{
    public class Settings
    {
        public const string SETTINGS_FILE = @"\SD\settings.txt";
        public const string PROBE_FILE = @"\SD\probe.txt";
        public const int MINUTES_MULTIPLIER = 60000;

        private int timeZoneOffset;

        public int TimeZoneOffset
        {
            get { return timeZoneOffset; }
            set { timeZoneOffset = value; }
        }

        private float minutesBetweenReadings;

        public float MinutesBetweenReadings
        {
            get { return minutesBetweenReadings * MINUTES_MULTIPLIER; }
            set { minutesBetweenReadings = value; }
        }

        private float temperatureOffset;

        public float TemperatureOffset
        {
            get { return temperatureOffset; }
            set { temperatureOffset = value; }
        }

        private float temperatureHeaterOffset;

        public float TemperatureHeaterOffset
        {
            get { return temperatureHeaterOffset; }
            set { temperatureHeaterOffset = value; }
        }

        private string historyFilename;

        public string HistoryFilename
        {
            get { return historyFilename; }
            set { historyFilename = value; }
        }

        private string ntpServerName;

        public string NTPServerName
        {
            get { return ntpServerName; }
            set { ntpServerName = value; }
        }
        
        private int minutesBetweenNTPUpdate;

        public int MinutesBetweenNTPUpdate
        {
            get { return minutesBetweenNTPUpdate * MINUTES_MULTIPLIER; }
            set { minutesBetweenNTPUpdate = value; }
        }
        
        private string netBiosName;

        public string NetBiosName
        {
            get { return netBiosName; }
            set { netBiosName = value; }
        }

        private bool enableDHCP;

        public bool EnableDHCP
        {
            get { return enableDHCP; }
            set { enableDHCP = value; }
        }

        private string staticIPAddress;

        public string StaticIPAddress
        {
            get { return staticIPAddress; }
            set { staticIPAddress = value; }
        }

        private string subnetMask;

        public string SubnetMask
        {
            get { return subnetMask; }
            set { subnetMask = value; }
        }

        private string defaultGateway;

        public string DefaultGateway
        {
            get { return defaultGateway; }
            set { defaultGateway = value; }
        }

        private string primaryDNSAddress;

        public string PrimaryDNSAddress
        {
            get { return primaryDNSAddress; }
            set { primaryDNSAddress = value; }
        }

        private string secondaryDNSAddress;

        public string SecondaryDNSAddress
        {
            get { return secondaryDNSAddress; }
            set { secondaryDNSAddress = value; }
        }

        private float voltageReference;

        public float VoltageReference
        {
            get { return voltageReference; }
            set { voltageReference = value; }
        }

        private float padResistance;

        public float PadResistance
        {
            get { return padResistance; }
            set { padResistance = value; }
        }

        private float resistanceRT;

        public float ResistanceRT
        {
            get { return resistanceRT; }
            set { resistanceRT = value; }
        }

        private float coefficientA;

        public float CoefficientA
        {
            get { return coefficientA; }
            set { coefficientA = value; }
        }

        private float coefficientB;

        public float CoefficientB
        {
            get { return coefficientB; }
            set { coefficientB = value; }
        }

        private float coefficientC;

        public float CoefficientC
        {
            get { return coefficientC; }
            set { coefficientC = value; }
        }

        private float  coefficientD;

        public float CoefficientD
        {
            get { return coefficientD; }
            set { coefficientD = value; }
        }


        public void loadSettings()
        {
            try
            {
                using (StreamReader file = new StreamReader(SETTINGS_FILE))
                {
                    string line;
                    string[] setting;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line != String.Empty)
                        {
                            float tempValue = 0.0F;
                            setting = line.Split('=');
                            if((setting != null) && (setting.Length > 1))
                            {
                                switch(setting[0])
                                {
                                    case "TimeZoneOffset":
                                        TimeZoneOffset = Convert.ToInt32(setting[1]);
                                        break;
                                    case "MinutesBetweenReadings":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            MinutesBetweenReadings = tempValue;
                                        }
                                        break;
                                    case "TemperatureOffset":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            TemperatureOffset = tempValue;
                                        }
                                        break;
                                    case "TemperatureHeaterOffset":
                                        if(TryParseFloat(setting[1], out tempValue))
                                        {
                                            TemperatureHeaterOffset = tempValue;
                                        }
                                        break;
                                    case "HistoryFilename":
                                        HistoryFilename = setting[1];
                                        break;
                                    case "NTPServerName":
                                        NTPServerName = setting[1];
                                        break;
                                    case "MinutesBetweenNTPUpdate":
                                        MinutesBetweenNTPUpdate = Convert.ToInt32(setting[1]);
                                        break;
                                    case "NetBiosName":
                                        NetBiosName = setting[1];
                                        break;
                                    case "EnableDHCP":
                                        if(setting[1].ToLower().Equals("true"))
                                        {
                                            EnableDHCP = true;
                                        }
                                        else
                                        {
                                            EnableDHCP = false;
                                        }
                                        break;
                                    case "StaticIPAddress":
                                        StaticIPAddress = setting[1];
                                        break;
                                    case "SubnetMask":
                                        SubnetMask = setting[1];
                                        break;
                                    case "DefaultGateway":
                                        DefaultGateway = setting[1];
                                        break;
                                    case "PrimaryDNSAddress":
                                        PrimaryDNSAddress = setting[1];
                                        break;
                                    case "SecondaryDNSAddress":
                                        SecondaryDNSAddress = setting[1];
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            try
            {
                using (StreamReader file = new StreamReader(PROBE_FILE))
                {
                    string line;
                    string[] setting;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line != String.Empty)
                        {
                            float tempValue = 0.0F;
                            setting = line.Split('=');
                            if ((setting != null) && (setting.Length > 1))
                            {
                                switch (setting[0])
                                {
                                    case "VoltageReference":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            VoltageReference = tempValue;
                                        }
                                        break;
                                    case "PadResistance":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            PadResistance = tempValue;
                                        }
                                        break;
                                    case "ResistanceRT":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            ResistanceRT = tempValue;
                                        }
                                        break;
                                    case "CoefficientA":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            CoefficientA = tempValue;
                                        }
                                        break;
                                    case "CoefficientB":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            CoefficientB = tempValue;
                                        }
                                        break;
                                    case "CoefficientC":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            CoefficientC = tempValue;
                                        }
                                        break;
                                    case "CoefficientD":
                                        if (TryParseFloat(setting[1], out tempValue))
                                        {
                                            CoefficientD = tempValue;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void saveSettings()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(SETTINGS_FILE, false))
                {
                    file.Write("TimeZoneOffset=");
                    file.WriteLine(timeZoneOffset.ToString());

                    file.Write("MinutesBetweenReadings=");
                    file.WriteLine(minutesBetweenReadings.ToString("f2"));

                    file.Write("TemperatureOffset=");
                    file.WriteLine(temperatureOffset.ToString("f2"));

                    file.Write("TemperatureHeaterOffset=");
                    file.WriteLine(temperatureHeaterOffset.ToString("f2"));

                    file.Write("HistoryFilename=");
                    file.WriteLine(historyFilename);

                    file.Write("NTPServerName=");
                    file.WriteLine(ntpServerName);

                    file.Write("MinutesBetweenNTPUpdate=");
                    file.WriteLine(minutesBetweenNTPUpdate.ToString());

                    file.Write("NetBiosName=");
                    file.WriteLine(netBiosName);

                    file.Write("EnableDHCP=");
                    file.WriteLine(EnableDHCP.ToString());

                    file.Write("StaticIPAddress=");
                    file.WriteLine(StaticIPAddress);

                    file.Write("SubnetMask=");
                    file.WriteLine(SubnetMask);

                    file.Write("DefaultGateway=");
                    file.WriteLine(DefaultGateway);

                    file.Write("PrimaryDNSAddress=");
                    file.WriteLine(PrimaryDNSAddress);

                    file.Write("SecondaryDNSAddress=");
                    file.WriteLine(SecondaryDNSAddress);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            try
            {
                using (StreamWriter file = new StreamWriter(PROBE_FILE, false))
                {
                    file.Write("VoltageReference=");
                    file.WriteLine(VoltageReference);

                    file.Write("PadResistance=");
                    file.WriteLine(PadResistance);

                    file.Write("ResistanceRT=");
                    file.WriteLine(ResistanceRT);

                    file.Write("CoefficientA=");
                    file.WriteLine(CoefficientA);

                    file.Write("CoefficientB=");
                    file.WriteLine(CoefficientB);

                    file.Write("CoefficientC=");
                    file.WriteLine(CoefficientC);

                    file.Write("CoefficientD=");
                    file.WriteLine(CoefficientD);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public static bool TryParseFloat(string s, out float result)
        {
            bool success = true;
            int decimalDiv = 0;
            result = 0;
            bool negative = false;
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '.' && decimalDiv == 0)
                    {
                        decimalDiv = 1;
                    }
                    else if (s[i] == '-')
                    {
                        negative = true;
                    }
                    else if (s[i] < '0' || s[i] > '9')
                    {
                        success = false;
                    }
                    else
                    {
                        result = result * 10;
                        decimalDiv = decimalDiv * 10;
                        result += (int)(s[i] - '0');
                    }
                }
                if (decimalDiv > 0)
                {
                    result = (float)result / decimalDiv;
                }
                if (negative)
                {
                    result = result * -1.0F;
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }

    }
}
