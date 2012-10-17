using System;
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

//
// SPI Vacum Flouresent Display Driver
// Based on the SPI_VFD Arduino Library
// Created by AdaFruit
//
namespace CodingSmackdown.Drivers
{
    // When the display powers up, it is configured as follows:
    //
    // 1. Display clear
    // 2. Function set: 
    //    N = 1; 2-line display 
    //    BR1=BR0=0; (100% brightness)
    // 3. Display on/off control: 
    //    D = 0; Display off 
    //    C = 0; Cursor off 
    //    B = 0; Blinking off 
    // 4. Entry mode set: 
    //    I/D = 1; Increment by 1 
    //    S = 0; No shift 
    //
    // Note, however, that resetting the Arduino doesn't reset the LCD, so we
    // can't assume that its in that state when a sketch starts (and the
    // SPI_VFD constructor is called).    
    public class SPI_VFDisplay : IDisposable
    {
        // commands
        private const byte VFD_CLEARDISPLAY = 0x01;
        private const byte VFD_RETURNHOME = 0x00;
        private const byte VFD_ENTRYMODESET = 0x04;
        private const byte VFD_DISPLAYCONTROL = 0x08;
        private const byte VFD_CURSORSHIFT = 0x10;
        private const byte VFD_FUNCTIONSET = 0x30;
        private const byte VFD_SETCGRAMADDR = 0x40;
        private const byte VFD_SETDDRAMADDR = 0x80;

        // flags for display entry mode
        private const byte VFD_ENTRYRIGHT = 0x00;
        private const byte VFD_ENTRYLEFT = 0x02;
        private const byte VFD_ENTRYSHIFTINCREMENT = 0x01;
        private const byte VFD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control
        private const byte VFD_DISPLAYON = 0x04;
        private const byte VFD_DISPLAYOFF = 0x00;
        private const byte VFD_CURSORON = 0x02;
        private const byte VFD_CURSOROFF = 0x00;
        private const byte VFD_BLINKON = 0x01;
        private const byte VFD_BLINKOFF = 0x00;

        // flags for display/cursor shift
        private const byte VFD_DISPLAYMOVE = 0x08;
        private const byte VFD_CURSORMOVE = 0x00;
        private const byte VFD_MOVERIGHT = 0x04;
        private const byte VFD_MOVELEFT = 0x00;

        // flags for function set
        private const byte VFD_2LINE = 0x08;
        private const byte VFD_1LINE = 0x00;

        private const byte VFD_SPICOMMAND = 0xF8;
        private const byte VFD_SPIDATA = 0xFA;

        private byte _linemode;
        private byte _brightness;
        private byte _displaycontrol;
        private byte _shiftmode;
        private byte _incrementmode;
        private byte _displaymode;
        private byte _cursormode;
        private byte _blinkmode;

        private byte _initialized;

        private byte _numlines;
        private byte _currline;

        private readonly SPI _spi;
        private readonly OutputPort _latchPort;
        readonly byte[] _writeBuf = new byte[1];

        public enum Brightness
        {
            VFD_BRIGHTNESS25 = 0x03,
            VFD_BRIGHTNESS50 = 0x02,
            VFD_BRIGHTNESS75 = 0x01,
            VFD_BRIGHTNESS100 = 0x00
        }

        public SPI_VFDisplay(SPI.SPI_module spiBus, Cpu.Pin latchPin, Brightness brightness)
        {
            var spiConfig = new SPI.Configuration(
                Cpu.Pin.GPIO_NONE, //latchPin,
                false, // active state
                0,     // setup time
                0,     // hold time 
                false, // clock idle state
                true,  // clock edge
                1000,   // clock rate
                spiBus);

            _spi = new SPI(spiConfig);

            _latchPort = new OutputPort(latchPin, true);

            //default to 2x20 display (SAMSUNG 20T202DA2JA)
            begin(20, 2, brightness);
        }

        public void begin(byte columns, byte lines, Brightness brightness)
        {
            // set number of lines
            if (lines > 1)
                _linemode = VFD_2LINE;
            else
                _linemode = VFD_1LINE;
            // save off for address translation
            _numlines = lines;

            // turn the display on with no cursor or blinking default
            _displaycontrol = VFD_DISPLAYON;
            _cursormode = VFD_CURSORON;
            _blinkmode = VFD_BLINKOFF;
            _shiftmode = VFD_ENTRYLEFT;
            _incrementmode = VFD_ENTRYSHIFTDECREMENT;
            // init the display
            display();

            //catch bad values
            if (brightness > Brightness.VFD_BRIGHTNESS25)
                _brightness = (byte)Brightness.VFD_BRIGHTNESS100;
            else
                _brightness = (byte)brightness;
            // set the brightness and push the linecount with VFD_SETFUNCTION
            setBrightness(brightness);

            // set cursor shift for romance lnaguages
            leftToRight();
            
            // clear the display
            clear();
            // set starting address to column 0, row 0
            home();
        }

        public void setBrightness(Brightness brightness)
        {
            // set the brightness (only if a valid value is passed
            if (brightness <= Brightness.VFD_BRIGHTNESS25)
            {
                _brightness = (byte)brightness;

                command((byte)(VFD_FUNCTIONSET | _brightness | _linemode));
            }
        }

        public byte getBrightness()
        {
            // get the brightness
            return (byte)(_brightness);
        }

        public void clear()
        {
            command(VFD_CLEARDISPLAY);  // clear display, set cursor position to zero
        }

        public void home()
        {
            command(VFD_SETDDRAMADDR | 0x00);  // set cursor position to zero
        }

        public void setCursor(byte col, byte row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };

            if (row > _numlines)
            {
                row = (byte)(_numlines - 1);    // we count rows starting w/0
            }
            // we only have 20 columns even though there are 40 cols addressable
            if (col > 20)
            {
                col = 20;
            }

            int address = col + row_offsets[row];

            command((byte)(VFD_SETDDRAMADDR | address));
        }

        // Turn the display on/off (quickly)
        public void noDisplay()
        {
            _displaycontrol = VFD_DISPLAYOFF;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        public void display()
        {
            _displaycontrol = VFD_DISPLAYON;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        // Turns the underline cursor on/off
        public void noCursor()
        {
            _cursormode = VFD_CURSOROFF;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        public void cursor()
        {
            _cursormode = VFD_CURSORON;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        // Turn on and off the blinking cursor
        public void noBlink()
        {
            _blinkmode = VFD_BLINKOFF;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        public void blink()
        {
            _blinkmode = VFD_BLINKON;
            command((byte)(VFD_DISPLAYCONTROL | _displaycontrol | _cursormode | _blinkmode));
        }

        // These commands scroll the display without changing the RAM
        public void scrollDisplayLeft()
        {
            command((byte)(VFD_CURSORSHIFT | VFD_DISPLAYMOVE | VFD_MOVELEFT));
        }

        public void scrollDisplayRight()
        {
            command((byte)(VFD_CURSORSHIFT | VFD_DISPLAYMOVE | VFD_MOVERIGHT));
        }

        // This is for text that flows Left to Right
        public void leftToRight()
        {
            _shiftmode = VFD_ENTRYLEFT;
            command((byte)(VFD_ENTRYMODESET | _shiftmode | _incrementmode));
        }

        // This is for text that flows Right to Left
        public void rightToLeft()
        {
            _shiftmode = VFD_ENTRYRIGHT;
            command((byte)(VFD_ENTRYMODESET | _shiftmode | _incrementmode));
        }

        // This will 'right justify' text from the cursor
        public void autoscroll()
        {
            _incrementmode = VFD_ENTRYSHIFTINCREMENT;
            command((byte)(VFD_ENTRYMODESET | _shiftmode | _incrementmode));
        }

        // This will 'left justify' text from the cursor
        public void noAutoscroll()
        {
            _incrementmode = VFD_ENTRYSHIFTDECREMENT;
            command((byte)(VFD_ENTRYMODESET | _shiftmode | _incrementmode));
        }

        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        public void createChar(byte location, byte[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            command((byte)(VFD_SETCGRAMADDR | (location << 3)));
            for (int i = 0; i < 8; i++)
            {
                write(charmap[i]);
            }
        }

        // spool string data to the display
        public void print(string data)
        {
            byte[] output = System.Text.Encoding.UTF8.GetBytes(data);
            byte[] buffer = new byte[] { VFD_SPIDATA, 0, VFD_SPIDATA, 0 };

            for (int i = 0; i < output.Length; i++)
            {
                _latchPort.Write(false);
                buffer[3] = output[i];
                _spi.Write(buffer);
                _latchPort.Write(true);
            }
        }

        /*********** mid level commands, for sending data/cmds, init */

        private void command(byte value)
        {
            _latchPort.Write(false);
            _spi.Write(new byte[] { VFD_SPICOMMAND, value, VFD_SPICOMMAND, value });
            _latchPort.Write(true);
        }

        private void write(byte value)
        {
            _latchPort.Write(false);
            _spi.Write(new byte[] { VFD_SPIDATA, value, VFD_SPIDATA, value });
            _latchPort.Write(true);
        }

        public void Dispose()
        {
            _spi.Dispose();
            _latchPort.Dispose();
        }
    }
}
