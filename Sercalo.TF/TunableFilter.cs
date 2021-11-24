using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sercalo.TF
{
    internal class TunableFilter : Sercalo.Serial.SerialDevice, ITunableFilter
    {
        #region CONSTANTS

        const string REGEX_DEFAULT_VALUE = "val";

        private static readonly int[] uartValues = new int[] { 9600, 19200, 38400, 57600, 115200 };
        public static readonly int DEFAULT_BAUDRATE = uartValues[0];

        private static readonly Parity[] uartParities = new Parity[] { Parity.None, Parity.Even, Parity.Odd, Parity.Mark, Parity.Space };
        public static readonly Parity DEFAULT_PARITY = uartParities[0];

        private static readonly char[] trimChars = new char[] { '\r', '\n', '\0' };

        #endregion

        #region INITIALISATION

        public TunableFilter()
            : base(DEFAULT_BAUDRATE, DEFAULT_PARITY) { }

        #endregion

        #region ITUNABLEFILTER FUNCTIONS

        /// <summary>
        /// Returns the equipment identification
        /// </summary>
        /// <returns></returns>
        public async Task<TunableFilterID> GetID()
        {
            string str = await QueryAsync("ID");
            return TunableFilterID.FromIDString(str);
        }

        /// <summary>
        /// Resets the device.
        /// </summary>
        public async Task<bool> Reset()
        {
            string str = await QueryAsync("RST");

            if (str != "RST")
                throw new TunableFilterException("Cannot reset the device");

            _port.BaudRate = DEFAULT_BAUDRATE;
            _port.Parity = DEFAULT_PARITY;

            await SleepLockAsync(1000);

            return true;
        }

        /// <summary>
        /// Gets the power mode.
        /// </summary>
        /// <returns></returns>
        public async Task<PowerMode> GetPowerMode()
            => (PowerMode)(await GetByte("POW"));

        /// <summary>
        /// Sets the power mode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public async Task<bool> SetPowerMode(PowerMode powerMode)
            => await SetByte("POW", (byte)powerMode);


        /// <summary>
        /// Gets the error mode.
        /// </summary>
        /// <returns></returns>
        public async Task<ErrorMode> GetErrorMode()
            => (ErrorMode)await GetByte("ERM");

        /// <summary>
        /// Sets the error mode.
        /// </summary>
        /// <param name="errorMode"></param>
        /// <returns></returns>
        public async Task<bool> SetErrorMode(ErrorMode errorMode)
            => await SetByte("ERM", (byte)errorMode);

        /// <summary>
        /// Gets the temperature of the microcontroller
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetTemperature()
            => await GetDouble("TMP");

        /// <summary>
        /// Gets the uart baud rate.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Sercalo.TF.TunableFilterException">Uart value out of range</exception>
        public async Task<int> GetUARTBaudRate()
        {
            byte uartId = await GetByte("UART");

            if (uartId >= uartValues.Length)
                throw new TunableFilterException("Uart value out of range");

            return uartValues[uartId];
        }

        /// <summary>
        /// Sets the uart baud rate.
        /// </summary>
        /// <param name="baudrate">The baudrate with either the id or the direct baudrate. Availabe values are:
        /// - 0: 9600 (default)
        /// - 1: 19200
        /// - 2: 38400
        /// - 3: 57600
        /// - 4: 115200</param>
        /// <returns></returns>
        public async Task<bool> SetUARTBaudRate(int baudrate)
        {
            for (int i = 0; i < uartValues.Length; i++)
            {
                if (baudrate == uartValues[i])
                {
                    baudrate = i;
                    break;
                }    
            }

            string input = $"UART {baudrate}";
            await WriteAsync(input);
            await SleepLockAsync(200);

            ThrowIfError((await ReadAllAsync()).TrimEnd(trimChars));

            _port.BaudRate = uartValues[baudrate];

            return input == await QueryAsync(input);
        }

        /// <summary>
        /// Gets the uart parity.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Sercalo.TF.TunableFilterException">Uart parity out of range</exception>
        public async Task<Parity> GetUARTParity()
        {
            byte parityId = await GetByte("PTY");

            if (parityId >= uartParities.Length)
                throw new TunableFilterException("Uart parity out of range");

            return uartParities[parityId];
        }

        /// <summary>
        /// Sets the uart parity.
        /// </summary>
        /// <param name="parity">The parity.</param>
        /// <returns></returns>
        public async Task<bool> SetUARTParity(Parity parity)
        {
            int id = 0;

            for (; id < uartParities.Length; id++)
            {
                if (parity == uartParities[id])
                    break;
            }
            string input = $"PTY {id}";

            await WriteAsync(input);
            await SleepLockAsync(200);

            ThrowIfError((await ReadAllAsync()).TrimEnd(trimChars));
            _port.Parity = parity;

            return input == await QueryAsync(input);
        }

        /// <summary>
        /// Gets the address for SMBus/I2C.
        /// </summary>
        /// <returns></returns>
        public async Task<byte> GetI2CAddress()
            => await GetByte("IIC");

        /// <summary>
        /// Sets the address for SMBus/I2C.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public async Task<bool> SetI2CAddress(byte address)
            => await SetByte("IIC", address);

        /// <summary>
        /// Gets the current position of the MEMS mirror
        /// </summary>
        /// <returns></returns>
        public async Task<Point> GetPosition()
            => StringToPoint(await GetValue("POS"));

        /// <summary>
        /// Sets the MEMS mirror to the specified position
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public async Task<bool> SetPosition(Point xy)
            => await SetValue("SET", PointToString(xy));

        /// <summary>
        /// Gets the position of the specified user-defined channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<Point> GetChannelPosition(byte channel)
            => StringToPoint(await GetValue($"CHGET {channel}"));

        /// <summary>
        /// Sets the specified user-defined channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<bool> SetChannel(byte channel)
            => await SetByte("CHSET", channel);

        /// <summary>
        /// Sets the specified user-defined channel position (i.e. modify)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public async Task<bool> SetChannelPosition(byte channel, Point xy)
            => await SetValue($"CHMOD {channel}", PointToString(xy));

        /// <summary>
        /// Gets the output wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetWavelength()
            => await GetDouble("WVL");

        /// <summary>
        /// Sets the output wavelength.
        /// </summary>
        /// <param name="wavelength">The wavelength.</param>
        /// <returns></returns>
        public async Task<bool> SetWavelength(double wavelength)
        { 
            bool result = await SetDouble("WVL", wavelength);
            await SleepLockAsync(1);
            return result;
        }

        /// <summary>
        /// Gets the minimum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetMinimumWavelength()
            => await GetDouble("WVMIN");

        /// <summary>
        /// Gets the maximum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetMaximumWavelength()
            => await GetDouble("WVMAX");

        #endregion

        #region PRIVATE FUNCTIONS

        private void ThrowIfError(string output)
        {
            Match match = Regex.Match(output, @"^ERR (?>(?<n>\d+)|(?<t>.+))$");

            if (match.Success)
            {
                if (match.Groups["n"].Success)
                    throw new TunableFilterException((ErrorCode)byte.Parse(match.Groups["n"].Value));
                else
                    throw new TunableFilterException(match.Groups["t"].Value);
            }
        }

        /// <summary>
        /// Send the specified input and wait for an output asynchronously, then match the string with specified pattern that include the val value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        private async Task<string> QueryAndMatchAsync(string input, string pattern)
        {
            string output = (await QueryAsync(input)).TrimEnd(trimChars);

            Match match = Regex.Match(output, pattern);

            if (!match.Success)
            {
                ThrowIfError(output);

                throw new SercaloException($"Cannot find a suitable match from expresion '{output}'.");
            }

            return match.Groups[REGEX_DEFAULT_VALUE].Value;
        }

        /// <summary>
        /// Gets the value from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        private async Task<string> GetValue(string function)
            => await QueryAndMatchAsync(function, $"^{function} (?<{REGEX_DEFAULT_VALUE}>.+)$");

        /// <summary>
        /// Sets the value from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private async Task<bool> SetValue(string function, string value)
        {
            return value == await QueryAndMatchAsync($"{function} {value}", $"^{function} (?<{REGEX_DEFAULT_VALUE}>.+)$");
        }

        /// <summary>
        /// Gets the value in double from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        private async Task<double> GetDouble(string function)
            => double.Parse(await GetValue(function), System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets the value in double from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private async Task<bool> SetDouble(string function, double value)
            => await SetValue(function, value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        private async Task<byte> GetByte(string function)
            => byte.Parse(await GetValue(function), System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets the value in byte from its function
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private async Task<bool> SetByte(string function, byte value)
            => await SetValue(function, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        /// <summary>
        /// Convert a String to point.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        private Point StringToPoint(string str)
        {
            string[] strs = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (strs.Length != 4)
                throw new FormatException(str);

            return new Point(GetValueFromElectrodes(strs[0], strs[1]), GetValueFromElectrodes(strs[2], strs[3]));
        }

        /// <summary>
        /// Convert a Point to string.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        private string PointToString(Point point)
        {
            (int x_neg, int x_pos) = GetElectrodeValues(point.X);
            (int y_neg, int y_pos) = GetElectrodeValues(point.Y);

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3}", x_neg, x_pos, y_neg, y_pos);
        }

        /// <summary>
        /// Gets the electrode values from the int vlaue
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private (int neg, int pos) GetElectrodeValues(int value)
        {
            int neg = 0;
            int pos = 0;

            if (value >= 0) pos = value;
            else neg = -value;

            return (neg, pos);
        }

        /// <summary>
        /// Gets the value from electrodes.
        /// </summary>
        /// <param name="neg">The neg.</param>
        /// <param name="pos">The position.</param>
        /// <returns></returns>
        private int GetValueFromElectrodes(string neg, string pos)
        {
            int value;

            if (int.TryParse(neg, out value)
                && value > 0)
                return -value;

            value = int.Parse(pos, System.Globalization.CultureInfo.InvariantCulture);

            return value;
        }

        #endregion
    }
}
