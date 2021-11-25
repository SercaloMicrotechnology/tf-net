using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sercalo.Serial.Query
{
    public static class SerialQueryHelper
    {
        #region PUBLIC UNSAFE FUNCTIONS

        /// <summary>
        /// Writes the specified input.
        /// This function shall be called only inside <see cref="ISerialDevice.LockFunction(Action{SerialPort})"/> or <see cref="ISerialDevice.LockFunction{T}(Func{SerialPort, T})"/>
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns></returns>
        /// <exception cref="Sercalo.SercaloException">$"Cannot write input '{input}'., err</exception>
        public static void UnsafeWrite(this SerialPort p, string input)
        {
            if (!p.IsOpen)
                throw new SercaloException("Device is not connected");

            p.DiscardBuffers();
            p.WriteLine(input);
        }

        /// <summary>
        /// Send the specified input and wait for an output
        /// This function shall be called only inside <see cref="ISerialDevice.LockFunction(Action{SerialPort})"/> or <see cref="ISerialDevice.LockFunction{T}(Func{SerialPort, T})"/>
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns>
        /// The received message
        /// </returns>
        public static string UnsafeQuery(this SerialPort p, string input)
        {
            if (!p.IsOpen)
                throw new SercaloException("Device is not connected");

            p.DiscardBuffers();
            p.WriteLine(input);
            return p.ReadLine();
        }


        /// <summary>
        /// Reads all available data
        /// This function shall be called only inside <see cref="ISerialDevice.LockFunction(Action{SerialPort})"/> or <see cref="ISerialDevice.LockFunction{T}(Func{SerialPort, T})"/>
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns>
        /// The received message
        /// </returns>
        public static string UnsafeReadAll(this SerialPort p)
        {
            if (!p.IsOpen)
                throw new SercaloException("Device is not connected");

            string output = "";

            do
            {
                output += p.ReadExisting();
                System.Threading.Thread.Sleep(1);
            }
            while (p.BytesToRead > 0);

            return output;
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Opens teh device asynchronously.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static async Task<bool> OpenAsync(this ISerialDevice device, string portName)
        {
            return await Task.Run(() => device.Open(portName));
        }

        /// <summary>
        /// Writes the specified input asynchronoulsy.
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns></returns>
        /// <exception cref="Sercalo.SercaloException">$"Cannot write input '{input}'., err</exception>
        public static async Task WriteAsync(this ISerialDevice device, string input)
        {
            try
            {
                await LockFunctionAsync(device, p => UnsafeWrite(p, input));
            }
            catch (Exception err)
            {
                throw new Sercalo.SercaloException($"Cannot write input '{input}'.", err);
            }
        }

        /// <summary>
        /// Send the specified input and wait for an output asynchronously
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns>
        /// The received message
        /// </returns>
        public static async Task<string> QueryAsync(this ISerialDevice device, string input)
        {
            try
            {
                return await LockFunctionAsync(device, p => UnsafeQuery(p, input));
            }
            catch (Exception err)
            {
                throw new Sercalo.SercaloException($"Cannot get response from input '{input}'.", err);
            }
        }


        /// <summary>
        /// Reads all available data asynchronously
        /// </summary>
        /// <param name="input">The sent message.</param>
        /// <returns>
        /// The received message
        /// </returns>
        public static async Task<string> ReadAllAsync(this ISerialDevice device)
        {
            try
            {
                return await LockFunctionAsync(device, p => UnsafeReadAll(p));
            }
            catch (Exception err)
            {
                throw new Sercalo.SercaloException($"Cannot get response", err);
            }
        }

        #endregion

        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Discards read/write buffers.
        /// </summary>
        /// <returns></returns>
        private static void DiscardBuffers(this SerialPort p)
        {
            p.DiscardOutBuffer();
            p.DiscardInBuffer();
        }

        /// <summary>
        /// Locks the specified function for thread-safe instance to avoid multiple Serial call to the device.
        /// The specified function will wait until all other locked function finished or if the ThreadSafeLockTimout is raised.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="func">The function to lock</param>
        /// <returns>The function results</returns>
        public static async Task<T> LockFunctionAsync<T>(this ISerialDevice device, Func<SerialPort, T> func)
             => await Task.Run(() => device.LockFunction(func));

        /// <summary>
        /// Locks the specified function for thread-safe instance to avoid multiple Serial call to the device.
        /// The specified function will wait until all other locked function finished or if the ThreadSafeLockTimout is raised.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="func">The function to lock</param>
        public static async  Task LockFunctionAsync(this ISerialDevice device, Action<SerialPort> func)
            => await Task.Run(() => device.LockFunction(func));

        #endregion
    }
}
