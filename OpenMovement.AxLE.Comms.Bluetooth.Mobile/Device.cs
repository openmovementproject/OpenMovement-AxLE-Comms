using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;

namespace OpenMovement.AxLE.Comms.Bluetooth.Mobile
{
    public class Device : Interfaces.IDevice
    {
        public Plugin.BLE.Abstractions.Contracts.IDevice NativeDevice { get; }

        public string Id { get; }
        public string MacAddress { get; }
        public string Name { get; }
        public int Rssi { get; }

        public Device(Plugin.BLE.Abstractions.Contracts.IDevice device, string macAddress = null)
        {
            NativeDevice = device;

            Id = NativeDevice.Id.ToString();
            Name = NativeDevice.Name;
            Rssi = NativeDevice.Rssi;
            MacAddress = macAddress;
        }

        public async Task<Interfaces.IService> GetService(Guid service)
        {
            return new Service(await NativeDevice.GetServiceAsync(service));
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }

    public class Service : Interfaces.IService
    {
        public Plugin.BLE.Abstractions.Contracts.IService NativeService { get; }

        public Guid Id { get; }

        public Service(Plugin.BLE.Abstractions.Contracts.IService service)
        {
            NativeService = service;

            Id = NativeService.Id;
        }

        public async Task<Interfaces.ICharacteristic> GetCharacteristic(Guid charac)
        {
            return new Characteristic(await NativeService.GetCharacteristicAsync(charac));
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }

    public class Characteristic : Interfaces.ICharacteristic
    {
        public Plugin.BLE.Abstractions.Contracts.ICharacteristic NativeCharacteristic;

        public Guid Id { get; }

        public event EventHandler<byte[]> ValueUpdated;

        public Characteristic(Plugin.BLE.Abstractions.Contracts.ICharacteristic characteristic)
        {
            NativeCharacteristic = characteristic;

            Id = NativeCharacteristic.Id;

            NativeCharacteristic.ValueUpdated += CharacteristicValueChanged;
        }

        public async Task RegisterForIndications()
        {
            // TODO: Fix multiple updates if not correctly disconnected...
            await NativeCharacteristic.StartUpdatesAsync();
        }

        public async Task RegisterForNotifications()
        {
            await NativeCharacteristic.StartUpdatesAsync();
        }

        public async Task UnregisterForUpdates()
        {
            await NativeCharacteristic.StopUpdatesAsync();
        }

        public async Task<byte[]> Read()
        {
            return await NativeCharacteristic.ReadAsync();
        }

        public async Task Write(byte[] data)
        {
            await NativeCharacteristic.WriteAsync(data);
        }

        private void CharacteristicValueChanged(object sender, CharacteristicUpdatedEventArgs e)
        {
            ValueUpdated?.Invoke(this, e.Characteristic.Value);
        }

        public async void Dispose()
        {
            NativeCharacteristic.ValueUpdated -= CharacteristicValueChanged;
            await UnregisterForUpdates();
        }
    }
}