using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercalo.Serial
{
    /// <summary>
    /// Interface for a Device using serial communication port
    /// </summary>
    public interface ISerialDevice: IDisposable
    {
        /// <summary>
        /// Gets the serial port used for this instance. 
        /// </summary>
        /// <remarks>
        /// The port is managed by the SerialDevice instance. Properties like baudrate, partiy and so could be requested for information but should not be changed.
        /// </remarks>
        SerialPort Port { get; }

        /// <summary>
        /// Gets or sets the thread safe lock timout (in milliseconds) used to restrain access to the serial port interface in multi-threaded instance
        /// </summary>
        /// <value>
        /// The thread safe lock timout in milliseconds
        /// </value>
        int ThreadSafeLockTimout { get; set; }

        /// <summary>
        /// Gets a value indicating whether this communication instance is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Opens this communication instance
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns><c>true</c> if this instance is open; otherwise, <c>false</c>.</returns>
        bool Open(string portName);

        /// <summary>
        /// Closes this communication instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Locks the specified function for thread-safe instance to avoid multiple Serial call to the device.
        /// The specified function will wait until all other locked function finished or if the ThreadSafeLockTimout is raised.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to lock</param>
        /// <returns>The function results</returns>
        T LockFunction<T>(Func<SerialPort, T> func);

        /// <summary>
        /// Locks the specified function for thread-safe instance to avoid multiple Serial call to the device.
        /// The specified function will wait until all other locked function finished or if the ThreadSafeLockTimout is raised.
        /// </summary>
        /// <param name="func">The function to lock</param>
        void LockFunction(Action<SerialPort> func);
    }
}
