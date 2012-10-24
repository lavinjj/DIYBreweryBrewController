using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CodingSmackdown.Services
{
    static public class PinManagement
    {
        static public bool alarmSounded = false;
        static public InterruptPort allStopButton = new InterruptPort(Pins.GPIO_PIN_D2, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public OutputPort buzzerPulsePort = new OutputPort(Pins.GPIO_PIN_D5, false);
        static public OutputPort buzzerSolidPort = new OutputPort(Pins.GPIO_PIN_D6, false);
        static public float currentTemperatureSensor = 0.0F;
        static public float currentPIDOuput = 0;
        static public bool autoTuning = false;

        // Define an Interrupt Port to watch when Pins D1, D2, D3, D4 Change States
        // Don't use a glitch filter
        // Use an external Pull Down resistor to hold the signal low until the switch is pressed.
        // Cause the interrupt to signal when the Pin changes from Low to High and from High to Low
        static public InterruptPort engageHeaterButton = new InterruptPort(Pins.GPIO_PIN_D1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        static public bool heaterEngaged = false;

        // define an output port that will drive the 12VDC Relay Circuit
        static public OutputPort heaterOnOffPort = new OutputPort(Pins.GPIO_PIN_D9, false);

        static public bool isHeating = false;
        static public MashSteps mashSteps = new MashSteps();
        static public float setTemperature = 75.0F;
        static public InterruptPort setTemperatureUpButton = new InterruptPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public InterruptPort setTemperatureUpDown = new InterruptPort(Pins.GPIO_PIN_D4, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static public float temperatureCelsiusSensor = 0.0F;
        static public DateTime currentMashStepStartTime = DateTime.Now; 
    }
}