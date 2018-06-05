using System;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using OpenMovement.AxLE.Comms.Bluetooth.Values;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Security.Cryptography;
using System.Collections.Generic;

namespace OpenMovement.AxLE.Comms.Bluetooth.Windows
{
    public class BluetoothManager : IBluetoothManager
    {
        private readonly BluetoothLEAdvertisementWatcher _watcher;

        private readonly ISet<ulong> _devicesDiscovered;

        private readonly System.Timers.Timer _scanTimeoutTimer;

        private Func<IDevice, bool> DeviceFilter { get; set; }

        public BluetoothScanMode ScanMode
        {
            get
            {
                switch (_watcher.ScanningMode)
                {
                    case BluetoothLEScanningMode.Active:
                        return BluetoothScanMode.LowLatency;
                    case BluetoothLEScanningMode.Passive:
                        return BluetoothScanMode.Passive;
                }
                return BluetoothScanMode.Other;
            }
            set
            {
                switch (value)
                {
                    case BluetoothScanMode.LowLatency:
                        _watcher.ScanningMode = BluetoothLEScanningMode.Active;
                        break;
                    case BluetoothScanMode.Passive:
                        _watcher.ScanningMode = BluetoothLEScanningMode.Passive;
                        break;
                    case BluetoothScanMode.LowPower:
                        _watcher.ScanningMode = BluetoothLEScanningMode.Passive;
                        break;
                }
            }
        }

        public int ScanTimeout { get; set; }

        public event EventHandler<IDevice> DeviceDiscovered;
        public event EventHandler<IDevice> DeviceAdvertised;
        public event EventHandler<IDevice> DeviceDisconnected; // NOT POSSIBLE ON UWP
        public event EventHandler<IDevice> DeviceConnectionLost; // NOT POSSIBLE ON UWP
        public event EventHandler<BluetoothState> StateChanged; // NOT POSSIBLE ON UWP
        public event EventHandler ScanTimeoutElapsed;

        public BluetoothManager()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };

            _watcher.Received += AdvertisementRecieved;

            ScanTimeout = 10000;

            _scanTimeoutTimer = new System.Timers.Timer
            {
                Interval = ScanTimeout,
                AutoReset = false
            };

            _scanTimeoutTimer.Elapsed += (s, args) =>
            {
                StopScan();
                ScanTimeoutElapsed?.Invoke(this, null);
            };

            _devicesDiscovered = new HashSet<ulong>();
        }

        public Task StartScan(Guid[] serviceGuids, Func<IDevice, bool> filter)
        {
            DeviceFilter = filter;
            _watcher.Start();
            _scanTimeoutTimer.Start();
            return Task.CompletedTask;
        }

        public Task StopScan()
        {
            _watcher.Stop();
            _scanTimeoutTimer.Stop();
            return Task.CompletedTask;
        }

        public async Task ConnectToDevice(IDevice device)
        {
            var nativeDevice = (Device) device;
            var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(nativeDevice.BluetoothAddress);
            nativeDevice.UpdateAfterConnect(bleDevice);
        }

        public async Task<IDevice> ConnectToKnownDevice(string deviceId, CancellationToken ct)
        {
            var bleDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            var device = new Device(bleDevice.BluetoothAddress, bleDevice.Name, 40 /* Fix for no RSSI on BLE Device object */);
            device.UpdateAfterConnect(bleDevice);
            return device;
        }

        public Task DisconnectDevice(IDevice device)
        {
            var nativeDevice = (Device) device;
            nativeDevice.NativeDevice.Dispose(); // There is no disconnect option...
            return Task.CompletedTask;
        }

        private void AdvertisementRecieved(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var device = new Device(args.BluetoothAddress, args.Advertisement.LocalName, args.RawSignalStrengthInDBm);
            if (DeviceFilter(device))
            {
                if (!_devicesDiscovered.Contains(device.BluetoothAddress))
                {
                    DeviceDiscovered?.Invoke(this, device);
                }
                else
                {
                    DeviceAdvertised?.Invoke(this, device);
                }
            }
        }

        public void Dispose()
        {
            _watcher.Stop();
            _watcher.Received -= AdvertisementRecieved;
        }
    }
}