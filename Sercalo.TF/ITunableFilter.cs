using System;
using System.Threading.Tasks;

namespace Sercalo.TF
{
    public interface ITunableFilter
    {
        /// <summary>
        /// Gets the string identifier asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<TunableFilterID> GetIDAsync();


//0x01 ID Returns the equipment identification
//0x02 RST Resets the device
//0x03 POW Returns or changes the power mode of the device
//0x04 ERM Returns or changes the error returning mode
//0x08 TMP Returns the temperature of the microcontroller
//0x10 UART Returns or changes the baud rate of the UART
//0x11 PTY Returns or changes the UART parity
//0x20 IIC Returns or changes the address for SMBus/I2C
//0x50 SET Sets the network configuration
//0x51 POS Returns the network configuration
//0x52 CHSET Sets the specified user-defined channel
//0x53 CHGET Returns the position of the specified user-defined channel
//0x54 CHMOD Modifies the specified user-defined channel
//0x55 WVL Returns or sets the output wavelength
//0x56 WVMIN Returns the minimum selectable wavelength
//0x57 WVMAX Returns the maximum selectable wavelength
    }
}
