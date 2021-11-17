using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sercalo.Serial
{
    internal abstract class SerialDevice : ISerialDevice
    {
        #region VARIABLES

        SerialPort _port = new("COM1", 9600, Parity.None, 8, StopBits.One);
        Mutex _lock = new Mutex(false, "SerialDeviceMutex");

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the thread safe lock timout (in milliseconds) used to restrain access to the serial port interface in multi-threaded instance
        /// </summary>
        /// <value>
        /// The thread safe lock timout in milliseconds
        /// </value>
        public int ThreadSafeLockTimout { get; set; } = 1000;

        /// <summary>
        /// Gets a value indicating whether this communication instance is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen => _port.IsOpen;

        #endregion

        #region INITIALISATION

        /// <summary>
        /// Opens this communication instance
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public bool Open(string portName)
        {
            try
            {
                return LockFunction(() =>
                {
                    _port.PortName = portName;
                    _port.Open();
                    return _port.IsOpen;
                });
            }
            catch (Exception err)
            {
                throw new SercaloException($"Cannot open the device with specified port '{portName}'.", err);
            }
        }

        public async Task<bool> OpenAsync(string portName)
        {
            return await Task.Run(() => Open(portName));
        }

        #endregion

        #region DISPOSE

        /// <summary>
        /// Finalizes an instance of the <see cref="SerialDevice"/> class.
        /// </summary>
        ~SerialDevice()
        {
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Closes this communication instance.
        /// </summary>
        public void Close()
        {
            if (_port.IsOpen)
            {
                LockFunction(() =>
                {
                    _port.Close();
                });
            }
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Send the specified input and wait for an output
        /// </summary>
        /// <param name="input">The sent message</param>
        /// <returns>
        /// The received message.
        /// </returns>
        /// <exception cref="Sercalo.SercaloException">Cannot get response from input '{input}'.</exception>
        public string Query(string input)
        {
            try
            {
                return LockFunction(() =>
                {
                    _port.WriteLine(input);
                    return _port.ReadLine();
                });
            }
            catch (Exception err)
            {
                throw new Sercalo.SercaloException($"Cannot get response from input '{input}'.", err);
            }
        }

        /// <summary>
        /// Send the specified input and wait for an output asynchronously
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns>
        /// The received message
        /// </returns>
        public async Task<string> QueryAsync(string input)
        {
            return await Task.Run(() => Query(input));
        }

        #endregion

        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Locks the specified function for thread-safe instance
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to lock</param>
        /// <returns>The function results</returns>
        /// <exception cref="System.TimeoutException">A thread-safe lock request failed to gain access.</exception>
        private T LockFunction<T>(Func<T> func)
        {
            if (!_lock.WaitOne(1000))
                throw new TimeoutException("A thread-safe lock request failed to gain access.");

            try
            {
                return func();
            }
            catch { throw; }
            finally
            {
                _lock.ReleaseMutex();
            }
        }

        /// <summary>
        /// Locks the specified function for thread-safe instance
        /// </summary>
        /// <param name="func">The function to lock</param>
        /// <exception cref="System.TimeoutException">A thread-safe lock request failed to gain access.</exception>
        private void LockFunction(Action func)
        {
            if (!_lock.WaitOne(1000))
                throw new TimeoutException("A thread-safe lock request failed to gain access.");

            try
            {
                func();
            }
            catch { throw; }
            finally
            {
                _lock.ReleaseMutex();
            }
        }

        #endregion
    }
}
