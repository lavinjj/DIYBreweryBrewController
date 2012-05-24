using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CodingSmackdown.Services
{
    static public class PinManagement
    {
        static public float milliVolts = 0.0F;
        static public float temperatureCelcius = 0.0F;
        static public float setTemperature = 0.0F;
        static public float currentTemperature = 0.0F;
        static public bool heaterEngaged = false;
        static public bool isHeating = false;
        static public bool alarmSounded = false;

        // define an output port that will drive the 12VDC Relay Circuit
        static public OutputPort heaterOnOffPort = new OutputPort(Pins.GPIO_PIN_D0, false);
        static public OutputPort buzzerPort = new OutputPort(Pins.GPIO_PIN_D5, false);

        // Define an Interrupt Port to watch when Pins D1, D2, D3, D4 Change States
        // Don't use a glitch filter
        // Use an external Pull Down resistor to hold the signal low until the switch is pressed.
        // Cause the interrupt to signal when the Pin changes from Low to High and from High to Low
        static public InterruptPort engageHeaterButton = new InterruptPort(Pins.GPIO_PIN_D1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public InterruptPort allStopButton = new InterruptPort(Pins.GPIO_PIN_D2, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public InterruptPort setTemperatureUpButton = new InterruptPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public InterruptPort setTemperatureUpDown = new InterruptPort(Pins.GPIO_PIN_D4, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        
        //static public OutputPort Digital5 = new OutputPort(Pins.GPIO_PIN_D5, false);
        //static public OutputPort Digital6 = new OutputPort(Pins.GPIO_PIN_D6, false);
        //static public OutputPort Digital7 = new OutputPort(Pins.GPIO_PIN_D7, false);
        //static public OutputPort Digital8 = new OutputPort(Pins.GPIO_PIN_D8, false);
        //static public OutputPort Digital9 = new OutputPort(Pins.GPIO_PIN_D9, false);
        //static public OutputPort Digital10 = new OutputPort(Pins.GPIO_PIN_D10, false);
        //static public OutputPort Digital11= new OutputPort(Pins.GPIO_PIN_D11, false);
        //static public OutputPort Digital12 = new OutputPort(Pins.GPIO_PIN_D12, false);
        //static public OutputPort Digital13 = new OutputPort(Pins.GPIO_PIN_D13, false);
        //static public OutputPort OnboardLED = new OutputPort(Pins.ONBOARD_LED, false);

        static public SecretLabs.NETMF.Hardware.AnalogInput temperatureSensorPort = new SecretLabs.NETMF.Hardware.AnalogInput(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_A0);

    }
}
