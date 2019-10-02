using System;
using System.Linq;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Security.Cryptography;

namespace OpenMovement.AxLE.Comms.Bluetooth.Windows
{
    public class Device : IDevice
    {
        public ulong BluetoothAddress { get; }
        public BluetoothLEDevice NativeDevice { get; private set; }

        public string Id { get; private set; }
        public string MacAddress { get; }
        public string Name { get; }
        public int Rssi { get; }

        public Device(ulong bluetoothAddress, string name, int rssi)
        {
            BluetoothAddress = bluetoothAddress;
            Id = BluetoothAddress.ToString("X"); // TEMP fix for Windows Bluetooth stupidity
            MacAddress = BluetoothAddress.ToString("X");
            Name = name;
            Rssi = rssi;
        }

        public void UpdateAfterConnect(BluetoothLEDevice device)
        {
            NativeDevice = device;
            Id = NativeDevice.DeviceId;
        }

        public async Task<IService> GetService(Guid service)
        {
            var result = await NativeDevice.GetGattServicesForUuidAsync(service);
            if (result.Services.Count <= 0) throw new Comms.Exceptions.ConnectException(new Exception($"Service not found: {service}"));
            return new Service(result.Services.First());
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }

        private static Guid ToGuid(string src)
        {
            byte[] stringbytes = System.Text.Encoding.UTF8.GetBytes(src);
            byte[] hashedBytes = new System.Security.Cryptography
                .SHA1CryptoServiceProvider()
                .ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }
    }

    public class Service : IService
    {
        public GattDeviceService NativeService { get; }

        public Guid Id { get; }

        public Service(GattDeviceService service)
        {
            NativeService = service;

            Id = NativeService.Uuid;
        }

        public async Task<ICharacteristic> GetCharacteristic(Guid charac)
        {
            var result = await NativeService.GetCharacteristicsForUuidAsync(charac);
            if (result.Characteristics.Count <= 0) throw new Comms.Exceptions.ConnectException(new Exception($"Characteristic not found: {charac}"));
            return new Characteristic(result.Characteristics.First());
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }

    public class Characteristic : ICharacteristic
    {
        public GattCharacteristic NativeCharacteristic { get; }

        public Guid Id { get; }

        public event EventHandler<byte[]> ValueUpdated;

        public Characteristic(GattCharacteristic charac)
        {
            NativeCharacteristic = charac;

            Id = NativeCharacteristic.Uuid;

            NativeCharacteristic.ValueChanged += (s, c) =>
            {
                var data = new byte[c.CharacteristicValue.Length];
                CryptographicBuffer.CopyToByteArray(c.CharacteristicValue, out data);
                ValueUpdated?.Invoke(this, data);
            };
        }

        public async Task RegisterForIndications()
        {
            await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
        }

        public async Task RegisterForNotifications()
        {
            await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        public async Task UnregisterForUpdates()
        {
            await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
        }

        public async Task Write(byte[] data)
        {
            var buffer = CryptographicBuffer.CreateFromByteArray(data);
            await NativeCharacteristic.WriteValueAsync(buffer);
        }

        public async Task<byte[]> Read()
        {
            var result = await NativeCharacteristic.ReadValueAsync();
            var data = new byte[result.Value.Length];
            CryptographicBuffer.CopyToByteArray(result.Value, out data);
            return data;
        }

        public void Dispose()
        {
            // TODO: Unregister for updates
        }
    }
}