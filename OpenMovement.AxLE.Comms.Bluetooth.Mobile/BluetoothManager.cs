using System;
using System.Threading;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using OpenMovement.AxLE.Comms.Bluetooth.Values;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OpenMovement.AxLE.Comms.Bluetooth.Mobile
{
    public class BluetoothManager : IBluetoothManager
    {
        private IBluetoothLE _ble;
        private IAdapter _adapter;

        public BluetoothScanMode ScanMode
        {
            get
            {
                switch (_adapter.ScanMode)
                {
                    case Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced:
                        return BluetoothScanMode.Balanced;
                    case Plugin.BLE.Abstractions.Contracts.ScanMode.LowLatency:
                        return BluetoothScanMode.LowLatency;
                    case Plugin.BLE.Abstractions.Contracts.ScanMode.LowPower:
                        return BluetoothScanMode.LowPower;
                    case Plugin.BLE.Abstractions.Contracts.ScanMode.Passive:
                        return BluetoothScanMode.Passive;
                    default:
                        return BluetoothScanMode.Other;
                }
            }
            set
            {
                switch (value)
                {
                    case BluetoothScanMode.Balanced:
                        _adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
                        break;
                    case BluetoothScanMode.LowLatency:
                        _adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowLatency;
                        break;
                    case BluetoothScanMode.LowPower:
                        _adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowPower;
                        break;
                    case BluetoothScanMode.Passive:
                        _adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Passive;
                        break;
                }
            }
        }

        public int ScanTimeout
        {
            get => _adapter.ScanTimeout;
            set
            {
                _adapter.ScanTimeout = value;
            }
        }

        public event EventHandler<Interfaces.IDevice> DeviceDiscovered;
        public event EventHandler<Interfaces.IDevice> DeviceAdvertised;
        public event EventHandler<Interfaces.IDevice> DeviceDisconnected;
        public event EventHandler<Interfaces.IDevice> DeviceConnectionLost;

        public event EventHandler<Values.BluetoothState> StateChanged;

        public event EventHandler ScanTimeoutElapsed;

        public BluetoothManager(IBluetoothLE ble)
        {
            _ble = ble;
            _adapter = _ble.Adapter;

            _ble.StateChanged += (o, s) =>
            {
                switch (s.NewState)
                {
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.On:
                        StateChanged?.Invoke(this, Values.BluetoothState.On);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.Off:
                        StateChanged?.Invoke(this, Values.BluetoothState.Off);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.TurningOn:
                        StateChanged?.Invoke(this, Values.BluetoothState.TurningOn);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.TurningOff:
                        StateChanged?.Invoke(this, Values.BluetoothState.TurningOff);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.Unavailable:
                        StateChanged?.Invoke(this, Values.BluetoothState.Unavailable);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.Unauthorized:
                        StateChanged?.Invoke(this, Values.BluetoothState.Unauthorized);
                        break;
                    case Plugin.BLE.Abstractions.Contracts.BluetoothState.Unknown:
                        StateChanged?.Invoke(this, Values.BluetoothState.Unknown);
                        break;
                }
            };

            _adapter.DeviceDiscovered += NativeDeviceDiscovered;
            _adapter.DeviceAdvertised += NativeDeviceAdvertised;
            _adapter.DeviceDisconnected += NativeDeviceDisconnected;
            _adapter.DeviceConnectionLost += NativeDeviceConnectionLost;
        }

        protected void NativeDeviceDiscovered(Interfaces.IDevice device)
        {
            DeviceDiscovered?.Invoke(this, device);
        }

        protected virtual void NativeDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            DeviceDiscovered?.Invoke(this, new Device(args.Device));
        }

        protected void NativeDeviceAdvertised(Interfaces.IDevice device)
        {
            DeviceAdvertised?.Invoke(this, device);
        }

        protected virtual void NativeDeviceAdvertised(object sender, DeviceEventArgs args)
        {
            DeviceAdvertised?.Invoke(this, new Device(args.Device));
        }

        protected void NativeDeviceDisconnected(Interfaces.IDevice device)
        {
            DeviceDisconnected?.Invoke(this, device);
        }

        protected virtual void NativeDeviceDisconnected(object sender, DeviceEventArgs args)
        {
            DeviceDisconnected?.Invoke(this, new Device(args.Device));
        }

        protected void NativeDeviceConnectionLost(Interfaces.IDevice device)
        {
            DeviceConnectionLost?.Invoke(this, device);
        }

        protected virtual void NativeDeviceConnectionLost(object sender, DeviceEventArgs args)
        {
            DeviceConnectionLost?.Invoke(this, new Device(args.Device));
        }

        public async Task StartScan(Guid[] serviceGuids, Func<Interfaces.IDevice, bool> filter = null)
        {
            if (filter == null)
            {
                await _adapter.StartScanningForDevicesAsync(serviceGuids);
            }
            else
            {
                await _adapter.StartScanningForDevicesAsync(serviceGuids, (d) => filter(new Device(d)));
            }
        }

        public async Task StopScan()
        {
            await _adapter.StopScanningForDevicesAsync();
        }

        public async Task ConnectToDevice(Interfaces.IDevice device)
        {
            await _adapter.ConnectToDeviceAsync(((Device) device).NativeDevice);
        }

        public async Task<Interfaces.IDevice> ConnectToKnownDevice(string deviceId, CancellationToken ct)
        {
            var device = await _adapter.ConnectToKnownDeviceAsync(new Guid(deviceId), new Plugin.BLE.Abstractions.ConnectParameters(), ct);
            return new Device(device);
        }

        public async Task DisconnectDevice(Interfaces.IDevice device)
        {
            await _adapter.DisconnectDeviceAsync(((Device)device).NativeDevice);
        }

        public void Dispose()
        {
            _adapter.DeviceDiscovered -= NativeDeviceDiscovered;
            _adapter.DeviceAdvertised -= NativeDeviceAdvertised;
            _adapter.DeviceDisconnected -= NativeDeviceDisconnected;
            _adapter.DeviceConnectionLost -= NativeDeviceConnectionLost;
        }
    }
}