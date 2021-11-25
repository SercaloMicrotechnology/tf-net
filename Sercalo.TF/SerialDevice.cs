using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sercalo.Serial
{
    internal abstract class SerialDevice : ISerialDevice
    {
        #region VARIABLES

        Mutex _lock = new Mutex(false, "SerialDeviceMutex");

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the serial port used for this instance. 
        /// </summary>
        /// <remarks>
        /// The port is managed by the SerialDevice instance. Properties like baudrate, partiy and so could be requested for information but should not be changed.
        /// </remarks>
        public SerialPort Port { get; private set; }

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
        public bool IsOpen => Port.IsOpen;

        #endregion

        #region INITIALISATION

        public SerialDevice(int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            Port = new SerialPort("COM1", baudRate, parity, dataBits, stopBits);
            Port.NewLine = "\r\n";
            Port.ReadTimeout = 5000;
            Port.WriteTimeout = 5000;
        }

        /// <summary>
        /// Opens this communication instance
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public bool Open(string portName)
        {
            try
            {
                return LockFunction(p =>
                {
                    p.PortName = portName;
                    p.Open();
                    return p.IsOpen;
                });
            }
            catch (Exception err)
            {
                throw new SercaloException($"Cannot open the device with specified port '{portName}'.", err);
            }
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
            if (Port.IsOpen)
            {
                LockFunction(p =>
                {
                    p.Close();
                });
            }
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Locks the specified function for thread-safe instance
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to lock</param>
        /// <returns>The function results</returns>
        /// <exception cref="System.TimeoutException">A thread-safe lock request failed to gain access.</exception>
        public T LockFunction<T>(Func<SerialPort, T> func)
        {
            if (!_lock.WaitOne(ThreadSafeLockTimout))
                throw new TimeoutException("A thread-safe lock request failed to gain access.");

            try
            {
                return func(Port);
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
        public void LockFunction(Action<SerialPort> func)
        {
            if (!_lock.WaitOne(ThreadSafeLockTimout))
                throw new TimeoutException("A thread-safe lock request failed to gain access.");

            try
            {
                func(Port);
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
