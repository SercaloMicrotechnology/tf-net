using System.ComponentModel;

namespace Sercalo.TF
{
    /// <summary>
    /// Device power mode enumeration
    /// </summary>
    public enum PowerMode : byte
    {
        /// <summary>
        /// The low power mode value
        /// </summary>
        Low = 0,
        /// <summary>
        /// The normal power mode value
        /// </summary>
        Normal = 1
    }

    /// <summary>
    /// Error Returning mode enumeration
    /// </summary>
    public enum ErrorMode : byte
    {
        /// <summary>
        /// Error are returned as number
        /// </summary>
        Number = 0,
        /// <summary>
        /// Error are return as text
        /// </summary>
        Text = 1
    }

    /// <summary>
    /// Error code enumeration
    /// </summary>
    public enum ErrorCode : byte
    {
        [Description("Error code unknown")]
        Unknown = 0,
        [Description("The CRC of the last SMBus/I2C command is invalid")]
        CRC = 2,
        [Description("Invalid parameter(s)")]
        InvalidParameters = 3,
        [Description("Command unknown")]
        UnknownCommand = 4,
        [Description("The command is too long and UART or SMBus/I2C receive buffer is full")]
        BufferOverrun = 6,
        [Description("Command unavailable because the device is in idle mode")]
        IdleMode = 8,
        [Description("The memory location of the selected channel is empty")]
        InvalidChannel = 9,
        [Description("Current wavelength is unknown")]
        InvalidWavelength = 10
    }
}