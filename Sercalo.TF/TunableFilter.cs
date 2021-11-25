using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sercalo.TF.Query;
using Sercalo.Serial.Query;

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
            string str = await this.QueryAsync("ID");
            return TunableFilterID.FromIDString(str);
        }

        /// <summary>
        /// Resets the device.
        /// </summary>
        public async Task<bool> Reset()
        {
            return await this.LockFunctionAsync(p =>
            {
                string str = p.UnsafeQuery("RST");

                if (str != "RST")
                    throw new TunableFilterException("Cannot reset the device");

                p.BaudRate = DEFAULT_BAUDRATE;
                p.Parity = DEFAULT_PARITY;

                Thread.Sleep(1000);

                return true;
            });
        }

        /// <summary>
        /// Gets the power mode.
        /// </summary>
        /// <returns></returns>
        public async Task<PowerMode> GetPowerMode()
            => (PowerMode)(await this.GetValue<byte>("POW"));

        /// <summary>
        /// Sets the power mode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public async Task<bool> SetPowerMode(PowerMode powerMode)
            => await this.SetValue("POW", (byte)powerMode);


        /// <summary>
        /// Gets the error mode.
        /// </summary>
        /// <returns></returns>
        public async Task<ErrorMode> GetErrorMode()
            => (ErrorMode)await this.GetValue<byte>("ERM");

        /// <summary>
        /// Sets the error mode.
        /// </summary>
        /// <param name="errorMode"></param>
        /// <returns></returns>
        public async Task<bool> SetErrorMode(ErrorMode errorMode)
            => await this.SetValue("ERM", (byte)errorMode);

        /// <summary>
        /// Gets the temperature of the microcontroller
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetTemperature()
            => await this.GetValue<double>("TMP");

        /// <summary>
        /// Gets the uart baud rate.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Sercalo.TF.TunableFilterException">Uart value out of range</exception>
        public async Task<int> GetUARTBaudRate()
        {
            byte uartId = await this.GetValue<byte>("UART");

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

            return await this.LockFunctionAsync(p =>
            {
                string input = $"UART {baudrate}";
                p.UnsafeWrite(input);
                Thread.Sleep(200);

                this.ApproveResult(p.UnsafeReadAll().TrimEnd(trimChars));

                p.BaudRate = uartValues[baudrate];

                return input == p.UnsafeQuery(input);
            });
        }

        /// <summary>
        /// Gets the uart parity.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Sercalo.TF.TunableFilterException">Uart parity out of range</exception>
        public async Task<Parity> GetUARTParity()
        {
            byte parityId = await this.GetValue<byte>("PTY");

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

            return await this.LockFunctionAsync(p =>
            {
                string input = $"PTY {id}";
                p.UnsafeWrite(input);
                Thread.Sleep(200);

                this.ApproveResult(p.UnsafeReadAll().TrimEnd(trimChars));

                Port.Parity = parity;

                return input == p.UnsafeQuery(input);
            });
        }

        /// <summary>
        /// Gets the address for SMBus/I2C.
        /// </summary>
        /// <returns></returns>
        public async Task<byte> GetI2CAddress()
            => await this.GetValue<byte>("IIC");

        /// <summary>
        /// Sets the address for SMBus/I2C.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public async Task<bool> SetI2CAddress(byte address)
            => await this.SetValue("IIC", address);

        /// <summary>
        /// Gets the current position of the MEMS mirror
        /// </summary>
        /// <returns></returns>
        public async Task<Point> GetPosition()
            => await this.GetXY("POS");

        /// <summary>
        /// Sets the MEMS mirror to the specified position
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public async Task<bool> SetPosition(Point xy)
            => await this.SetXY("SET", xy);

        /// <summary>
        /// Gets the position of the specified user-defined channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<Point> GetChannelPosition(byte channel)
            => await this.GetXY($"CHGET {channel}");

        /// <summary>
        /// Sets the specified user-defined channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<bool> SetChannel(byte channel)
            => await this.SetValue("CHSET", channel);

        /// <summary>
        /// Sets the specified user-defined channel position (i.e. modify)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public async Task<bool> SetChannelPosition(byte channel, Point xy)
            => await this.SetXY($"CHMOD {channel}", xy);

        /// <summary>
        /// Gets the output wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetWavelength()
            => await this.GetValue<double>("WVL");

        /// <summary>
        /// Sets the output wavelength.
        /// </summary>
        /// <param name="wavelength">The wavelength.</param>
        /// <returns></returns>
        public async Task<bool> SetWavelength(double wavelength)
        { 
            bool result = await this.SetValue("WVL", wavelength);
            Thread.Sleep(1);
            return result;
        }

        /// <summary>
        /// Gets the minimum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetMinimumWavelength()
            => await this.GetValue<double>("WVMIN");

        /// <summary>
        /// Gets the maximum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetMaximumWavelength()
            => await this.GetValue<double>("WVMAX");

        #endregion
    }
}
