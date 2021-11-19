using System;
using Xunit;
using Sercalo.TF;
using System.Threading.Tasks;
using Sercalo.TF.UnitTests.Attributes;
using System.Drawing;

namespace Sercalo.TF.UnitTests
{
    [TestCaseOrderer("Sercalo.TF.UnitTests.Orderers.PriorityOrderer", "Sercalo.TF.UnitTests")]
    [CollectionDefinition(nameof(Test_TF), DisableParallelization = true)]
    public class Test_TF
    {
        static ITunableFilter _device;
        static byte _address;
        static Point xy = new Point(5000, -5000);
        static double minWavelength, maxWavelength;
        static int setWavelength;

        [Fact, TestPriority(0)]
        public void TestCreation()
        {
            _device = Sercalo.TF.Factory.Create();

            Assert.NotNull(_device);
        }

        [Fact, TestPriority(1)]
        public async Task TestConnectionFailedAsync()
        {
            await Assert.ThrowsAsync<SercaloException>(() => _device.OpenAsync("wrongport"));
        }

        [Fact, TestPriority(2)]
        public async Task TestConnectAsync()
        {
            bool test = await _device.OpenAsync("COM3");

            Assert.True(test, "Cannot connect to device");
        }

        [Fact, TestPriority(3)]
        public async Task TestFailIdleMode()
        {
            TunableFilterException err = await Assert.ThrowsAsync<TunableFilterException>(() => _device.GetWavelength());

            bool test = await _device.SetErrorMode(ErrorMode.Number);

            Assert.True(test, "Cannot change error mode");

            err = await Assert.ThrowsAsync<TunableFilterException>(() => _device.GetWavelength());

            Assert.Equal(ErrorCode.IdleMode, err.Code);
        }

        [Fact, TestPriority(4)]
        public async Task TestUART()
        {
            bool test = await _device.SetUARTBaudRate(4);

            Assert.True(test, "Cannot set Uart baudrate 4");

            int baudrate = await _device.GetUARTBaudRate();

            Assert.Equal(115200, baudrate);

            test = await _device.SetUARTBaudRate(9600);

            Assert.True(test, "Cannot set Uart baudrate 9600");

            await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetUARTBaudRate(93743));
        }


        [Fact, TestPriority(5)]
        public async Task TestReset()
        {
            bool test = await _device.Reset();

            Assert.True(test, "Cannot reset the device");
        }


        [Fact, TestPriority(6)]
        public async Task TestPower()
        {
            bool test = await _device.SetPowerMode(PowerMode.Normal);
            
            Assert.True(test, "Cannot set power mode");

            PowerMode mode = await _device.GetPowerMode();

            Assert.Equal(PowerMode.Normal, mode);
        }

        [Fact, TestPriority(10)]
        public async Task TestGetID()
        {
            var data = await _device.GetID();

            Assert.Equal("TF", data.ProductName);
        }

        [Fact, TestPriority(30)]
        public async Task TestTemperature()
        {
            var data = await _device.GetTemperature();

            Assert.InRange(data, 0.0, 50.0);
        }

        [Fact, TestPriority(20)]
        public async Task TestI2CAddressGet()
        {
            _address = await _device.GetI2CAddress();

            bool test = await _device.SetI2CAddress(125);

            Assert.True(test, "Cannot set I2C address 125");

            await _device.SetI2CAddress(_address);

            Assert.True(test, $"Cannot set I2C address {_address}");
        }

        [Fact, TestPriority(100)]
        public async Task TestPositionSet()
        {
            Assert.True(await _device.SetPosition(xy), "Cannot set position with specified xy values");
        }

        [Fact, TestPriority(101)]
        public async Task TestPositionGet()
        {
            Point xy2 = await _device.GetPosition();

            Assert.Equal(xy, xy2);
        }

        [Fact, TestPriority(103)]
        public async Task TestPositionSetFailed()
        {
            await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetPosition(67000, 0));
        }

        [Fact, TestPriority(120)]
        public async Task TestChannelSetPostition()
        {
            Assert.True(await _device.SetChannelPosition(1, xy), "Cannot modify postion");
        }

        [Fact, TestPriority(125)]
        public async Task TestChannelSetZero()
        {
            Assert.True(await _device.SetChannelPosition(1, 0, 0), "Cannot modify channel to zero");
        }

        [Fact, TestPriority(121)]
        public async Task TestChannelGetPosition()
        {
            Point xy2 = await _device.GetChannelPosition(1);

            Assert.Equal(xy, xy2);
        }

        [Fact, TestPriority(122)]
        public async Task TestChannelSet()
        {
            Assert.True(await _device.SetChannel(1), "Cannot set channel");
        }

        [Fact, TestPriority(124)]
        public async Task TestChannelSetPositionFailed()
        {
            await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetChannelPosition(3, 67000, 0));
        }

        //[Fact, TestPriority(125)]
        //public async Task TestChannelSetFailed()
        //{
        //    await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetChannel(200));
        //}

        [Fact, TestPriority(200)]
        public async Task TestMinWavelength()
        {
            minWavelength = await _device.GetMinimumWavelength();

            Assert.InRange(minWavelength, 1000, 2000);
        }

        [Fact, TestPriority(201)]
        public async Task TestMaxWavelength()
        {
            maxWavelength = await _device.GetMaximumWavelength();

            Assert.InRange(maxWavelength, minWavelength, 2000);
        }

        [Fact, TestPriority(202)]
        public async Task TestSetWavelength()
        {
            setWavelength = (int)Math.Round((2 * minWavelength + maxWavelength) / 3);

            bool test = await _device.SetWavelength(setWavelength);

            Assert.True(test, $"Cannot set wavelength {setWavelength}");
        }

        [Fact, TestPriority(203)]
        public async Task TestGetWavelength()
        {
            int value = (int)(await _device.GetWavelength());

            Assert.Equal(setWavelength, value);
        }

        [Fact, TestPriority(204)]
        public async Task TestSetWavelengthLowFailed()
        {
            await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetWavelength(minWavelength - 1));
        }

        [Fact, TestPriority(205)]
        public async Task TestSetWavelengthHighFailed()
        {
            await Assert.ThrowsAsync<TunableFilterException>(() => _device.SetWavelength(maxWavelength + 1));
        }

        [Fact, TestPriority(950)]
        public async Task TestPowerOff()
        {
            bool test = await _device.SetPowerMode(PowerMode.Low);

            Assert.True(test, "Cannot set power mode");

            PowerMode mode = await _device.GetPowerMode();

            Assert.Equal(PowerMode.Low, mode);
        }

        [Fact, TestPriority(1000)]
        public void TestClose()
        {
            _device.Close();

            Assert.False(_device.IsOpen, "Cannot close device");
        }
    }
}
