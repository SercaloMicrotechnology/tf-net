using System;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;

namespace Sercalo.TF
{
    /// <summary>
    /// Interface for a Sercalo Tunable filter using Asynchronous call
    /// </summary>
    /// <seealso cref="Sercalo.Serial.ISerialDevice" />
    public interface ITunableFilter : Sercalo.Serial.ISerialDevice
    {
        /// <summary>
        /// Returns the equipment identification
        /// </summary>
        /// <returns></returns>
        Task<TunableFilterID> GetID();

        /// <summary>
        /// Resets the device .
        /// </summary>
        /// <returns></returns>
        Task<bool> Reset();

        /// <summary>
        /// Gets the power mode.
        /// </summary>
        /// <returns></returns>
        Task<PowerMode> GetPowerMode();

        /// <summary>
        /// Sets the power mode .
        /// </summary>
        /// <param name="value">The power mode.</param>
        /// <returns></returns>
        Task<bool> SetPowerMode(PowerMode powerMode);

        /// <summary>
        /// Gets the error mode.
        /// </summary>
        /// <returns></returns>
        Task<ErrorMode> GetErrorMode();

        /// <summary>
        /// Sets the error mode.
        /// </summary>
        /// <param name="value">The error mode.</param>
        /// <returns></returns>
        Task<bool> SetErrorMode(ErrorMode errorMode);

        /// <summary>
        /// Gets the temperature of the microcontroller
        /// </summary>
        /// <returns></returns>
        Task<double> GetTemperature();

        /// <summary>
        /// Gets the uart baud rate.
        /// </summary>
        /// <returns></returns>
        Task<int> GetUARTBaudRate();

        /// <summary>
        /// Sets the uart baud rate.
        /// </summary>
        /// <param name="baudrate">
        /// The baudrate with either the id or the direct baudrate. Availabe values are:
        ///     - 0: 9600 (default)
        ///     - 1: 19200
        ///     - 2: 38400
        ///     - 3: 57600
        ///     - 4: 115200
        /// </param>
        /// <returns></returns>
        Task<bool> SetUARTBaudRate(int baudrate);

        /// <summary>
        /// Gets the uart parity.
        /// </summary>
        /// <returns></returns>
        Task<Parity> GetUARTParity();

        /// <summary>
        /// Sets the uart parity.
        /// </summary>
        /// <param name="parity">The parity.</param>
        /// <returns></returns>
        Task<bool> SetUARTParity(Parity parity);

        /// <summary>
        /// Gets the address for SMBus/I2C.
        /// </summary>
        /// <returns></returns>
        Task<byte> GetI2CAddress();

        /// <summary>
        /// Sets the address for SMBus/I2C.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        Task<bool> SetI2CAddress(byte address);

        /// <summary>
        /// Gets the current position of the MEMS mirror
        /// </summary>
        /// <returns></returns>
        Task<Point> GetPosition();

        /// <summary>
        /// Sets the MEMS mirror to the specified position
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        Task<bool> SetPosition(Point xy);

        /// <summary>
        /// Gets the position of the specified user-defined channel
        /// </summary>
        /// <returns></returns>
        Task<Point> GetChannelPosition(byte channel);

        /// <summary>
        /// Sets the specified user-defined channel
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        Task<bool> SetChannel(byte channel);

        /// <summary>
        /// Sets the specified user-defined channel position (i.e. modify)
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        Task<bool> SetChannelPosition(byte channel, Point xy);

        /// <summary>
        /// Gets the output wavelength.
        /// </summary>
        /// <returns></returns>
        Task<double> GetWavelength();

        /// <summary>
        /// Sets the output wavelength.
        /// </summary>
        /// <param name="wavelength">The wavelength.</param>
        /// <returns></returns>
        Task<bool> SetWavelength(double wavelength);

        /// <summary>
        /// Gets the minimum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        Task<double> GetMinimumWavelength();

        /// <summary>
        /// Gets the maximum selectable wavelength.
        /// </summary>
        /// <returns></returns>
        Task<double> GetMaximumWavelength();
    }

    public static class TunableFilterHelper
    {
        /// <summary>
        /// Sets the MEMS mirror to the specified position
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public static Task<bool> SetPosition(this ITunableFilter device, int x, int y)
            => device.SetPosition(new Point(x, y));

        /// <summary>
        /// Sets the specified user-defined channel position (i.e. modify)
        /// </summary>
        /// <param name="xy">The requested position.</param>
        /// <returns></returns>
        public static Task<bool> SetChannelPosition(this ITunableFilter device, byte channel, int x, int y)
            => device.SetChannelPosition(channel, new Point(x, y));
    }
}
