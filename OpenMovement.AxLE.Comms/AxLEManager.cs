using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using OpenMovement.AxLE.Comms.Bluetooth.Values;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEManager : IAxLEManager
    {
        private const string AxLEDeviceName = "axLE-Band";
        private const string AxLEBootloaderDeviceName = "OM-DFU";
        private const int InterrogateInterval = 100;
        private const int NearbyInterval = 500;

        private static readonly Guid[] AxLEScanServiceUuids = { AxLEUuid.UartServiceUuid };
        private static readonly Guid[] AxLEScanBootloaderUuids = { AxLEUuid.BootloaderServiceUuid };

        private readonly IBluetoothManager _ble;

        private bool _interrogating;
        private ConcurrentQueue<IDevice> _interrogateQueue;
        private readonly System.Timers.Timer _interrogateTimer;

        private readonly System.Timers.Timer _nearbyTimer;
        private readonly int _nearbyTimeout;

        private IDictionary<string, IDevice> _devices = new Dictionary<string, IDevice>();
        private IDictionary<string, DateTime> _lastSeen = new Dictionary<string, DateTime>();

        public int RssiFilter { get; set; }

        public event EventHandler<string> DeviceFound;
        public event EventHandler<string> DeviceLost;
        public event EventHandler<string> DeviceDisconnected;

        public event EventHandler<IDevice> BootDeviceFound;
        public event EventHandler<IDevice> BootDeviceLost;
        public event EventHandler<IDevice> BootDeviceDisconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenMovement.AxLE.Comms.AxLEManager"/> class.
        /// </summary>
        /// <param name="ble">The Bluetooth manager to be used.</param>
        /// <param name="bleThread">If Bluetooth commands need to be run on a specific thread pass this here.</param>
        /// <param name="nearbyTimeout">The timeout before a device is considered lost, in milliseconds.</param>
        public AxLEManager(IBluetoothManager ble, int nearbyTimeout = 30000)
        {
            _ble = ble;

            _ble.ScanTimeout = 999999999;
            _ble.ScanTimeoutElapsed += async (s, a) => { await StartScan(); };

            _ble.StateChanged += StateChanged;
            _ble.DeviceDiscovered += AxLEDeviceDiscovered;
            _ble.DeviceAdvertised += AxLEDeviceAdvertised;
            _ble.DeviceDisconnected += AxLEDeviceDisconnected;
            _ble.DeviceConnectionLost += AxLEDeviceConnectionLost;

            _nearbyTimeout = nearbyTimeout;

            _nearbyTimer = new System.Timers.Timer
            {
                Interval = NearbyInterval,
                AutoReset = true
            };

            _nearbyTimer.Elapsed += UpdateDevicesNearby;

            _interrogateTimer = new System.Timers.Timer
            {
                Interval = InterrogateInterval,
                AutoReset = true
            };

            _interrogateTimer.Elapsed += ProcessInterrogationStack;

            _interrogateQueue = new ConcurrentQueue<IDevice>();

            RssiFilter = 80;
        }

        public async Task StartScan()
        {
            _interrogateTimer.Start();
            _nearbyTimer.Start();
			await _ble.StartScan(AxLEScanServiceUuids, (d) => d.Name == AxLEDeviceName);
		}

        public async Task ScanBootloader()
        {
            await _ble.StartScan(AxLEScanBootloaderUuids, (d) => d.Name == AxLEBootloaderDeviceName);
        }

        public async Task StopScan()
		{
			await _ble.StopScan();
            _interrogateTimer.Stop();
            _interrogateQueue = new ConcurrentQueue<IDevice>();
            _nearbyTimer.Stop();
            _lastSeen.Clear();
		}

        public void SwitchToHighPowerScan()
        {
            _ble.ScanMode = BluetoothScanMode.LowLatency;
        }

        public void SwitchToLowPowerScan()
        {
            _ble.ScanMode = BluetoothScanMode.LowPower;
        }

        public async Task SwitchToBootloaderMode()
        {
            await _ble.StopScan();

            _ble.DeviceDiscovered -= AxLEDeviceDiscovered;
            _ble.DeviceAdvertised -= AxLEDeviceAdvertised;
            _ble.DeviceDisconnected -= AxLEDeviceDisconnected;
            _ble.DeviceConnectionLost -= AxLEDeviceDisconnected;

            _ble.DeviceDiscovered += BootloaderDeviceDiscovered;
            _ble.DeviceDisconnected += BootloaderDeviceDisconnected;
            _ble.DeviceConnectionLost += BootloaderDeviceConnectionLost;
        }

        public async Task SwitchToNormalMode()
        {
            await _ble.StopScan();

            _ble.DeviceDiscovered -= BootloaderDeviceDiscovered;
            _ble.DeviceDisconnected -= BootloaderDeviceDisconnected;
            _ble.DeviceConnectionLost -= BootloaderDeviceConnectionLost;

            _ble.DeviceDiscovered += AxLEDeviceDiscovered;
            _ble.DeviceAdvertised += AxLEDeviceAdvertised;
            _ble.DeviceDisconnected += AxLEDeviceDisconnected;
            _ble.DeviceConnectionLost += AxLEDeviceDisconnected;
        }

        private void StateChanged(object sender, BluetoothState s)
        {
            Console.WriteLine($"BLUETOOTH STATE: {s}");
        }

        private void AxLEDeviceDiscovered(object sender, IDevice device)
        {
            ProcessDeviceDiscovered(device);
        }

        private void AxLEDeviceDisconnected(object sender, IDevice device)
        {
            var serial = _devices.SingleOrDefault(d => d.Value.Id == device.Id).Key;

            if (!string.IsNullOrEmpty(serial))
                DeviceDisconnected?.Invoke(this, serial);
        }

        private void AxLEDeviceConnectionLost(object sender, IDevice device)
        {
            var serial = _devices.SingleOrDefault(d => d.Value.Id == device.Id).Key;

            if (!string.IsNullOrEmpty(serial))
                DeviceDisconnected?.Invoke(this, serial);
        }

        private void BootloaderDeviceDiscovered(object sender, IDevice device)
        {
            BootDeviceFound?.Invoke(this, device);
        }

        private void BootloaderDeviceDisconnected(object sender, IDevice device)
        {
            BootDeviceDisconnected?.Invoke(this, device);
        }

        private void BootloaderDeviceConnectionLost(object sender, IDevice device)
        {
            BootDeviceDisconnected?.Invoke(this, device);
        }

        private void AxLEDeviceAdvertised(object sender, IDevice device)
        {
            if (device.Rssi > RssiFilter && !_lastSeen.ContainsKey(device.Id))
            {
                if (_devices.Any(d => d.Value.Id == device.Id))
                {
                    var devicePair = _devices.Single(d => d.Value.Id == device.Id);
                    DeviceFound?.Invoke(this, devicePair.Key);
                } else {
                    ProcessDeviceDiscovered(device);
                }
            }

            _lastSeen[device.Id] = DateTime.Now;
        }

        private void ProcessDeviceDiscovered(IDevice device)
        {
            if (device.Rssi > RssiFilter && _devices.Any(d => d.Value.Id == device.Id))
                return;

            if (string.IsNullOrEmpty(device.MacAddress))
            {
                if (!_interrogateQueue.Contains(device))
                {
                    _interrogateQueue.Enqueue(device);
                }
            } else 
            {
                _devices[device.MacAddress] = device;
                _lastSeen[device.Id] = DateTime.Now;
                DeviceFound?.Invoke(this, device.MacAddress);
            }
        }

        private async void ProcessInterrogationStack(object sender, ElapsedEventArgs e)
        {
            if (!_interrogating)
            {
                _interrogating = true;

                while (_interrogateQueue.Any())
                {
                    if (_interrogateQueue.TryDequeue(out IDevice device))
                    {
                        var serial = await InterrogateForSerial(device);

                        if (!string.IsNullOrEmpty(serial))
                        {
                            _devices[serial] = device;
                            _lastSeen[device.Id] = DateTime.Now;
                            DeviceFound?.Invoke(this, serial);
                        }
                    }
                }

                _interrogating = false;
            }
        }

        private async Task<string> InterrogateForSerial(IDevice device)
        {
            string serial = "";
            try
            {
                await _ble.ConnectToDevice(device);

                var diService = await device.GetService(AxLEUuid.DeviceInformationServiceUuid);
                var snCharac = await diService.GetCharacteristic(AxLEUuid.SerialNumberCharacUuid);
                var serialBytes = await snCharac.Read();
                serial = Encoding.UTF8.GetString(serialBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UNABLE TO CONNECT TO DEVICE: {ex}");
            }

            try
            {
                // May cause issues with multiple devices if you don't disconnect after each interrogation.
                await _ble.DisconnectDevice(device);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UNABLE TO DISCONNECT: {ex}");
            }

            return serial;
        }

        private void UpdateDevicesNearby(object sender, ElapsedEventArgs e)
        {
            foreach(var ls in _lastSeen.ToArray())
            {
                if ((DateTime.Now - ls.Value).TotalMilliseconds > _nearbyTimeout)
                {
                    var device = _devices.SingleOrDefault(d => d.Value.Id == ls.Key).Key;

                    _lastSeen.Remove(ls.Key);
                    DeviceLost?.Invoke(this, device);
                }
            }
        }

        public async Task<IAxLE> ConnectDevice(string serial)
        {
            if (!_devices.ContainsKey(serial))
                throw new DeviceNotInRangeException();

            var bleDevice = _devices[serial];
            _lastSeen.Remove(bleDevice.Id);
            // Only neccesary if we disconnect from each discovered device.
            await _ble.ConnectToDevice(bleDevice);

            return await CreateAxLE(bleDevice, serial);
        }

        public async Task<IAxLE> ConnectToKnownDevice(string serial, bool timeout = true)
        {
            var delayTask = Task.Delay(1000);

            var ct = new CancellationTokenSource();
            var device = _devices[serial];
            var conntectTask = _ble.ConnectToKnownDevice(device.Id, ct.Token);

            IDevice bleDevice;

            if (timeout)
            {
                var result = await Task.WhenAny(conntectTask, delayTask);

                if (result == delayTask)
                {
                    ct.Cancel();
                    throw new DeviceNotInRangeException();
                }

                bleDevice = (IDevice)result;
            } else
            {
                bleDevice = await conntectTask;
            }

            return await CreateAxLE(bleDevice, serial);
        }

        private async Task<IAxLE> CreateAxLE(IDevice device, string serial)
        {
            var axLE = new AxLEDevice(device);
            await axLE.OpenComms();

            if (axLE.HardwareVersion != 1.1)
                throw new DeviceIncompatibleException($"Hardware Version {axLE.HardwareVersion} is unsupported by this library.");

            switch (axLE.FirmwareVersion)
            {
                case 1.5:
                    return new AxLEv1_5(axLE, serial);
                default:
                    throw new DeviceIncompatibleException($"Firmware Version {axLE.FirmwareVersion} is unsupported by this library.");
            }
        }

        public async Task DisconnectDevice(IAxLE device)
        {
            device.Dispose();
            var bleDevice = _devices[device.SerialNumber];
            await _ble.DisconnectDevice(bleDevice);
        }

        public void Dispose()
        {
            _interrogateTimer.Stop();
            _interrogateTimer.Dispose();

            _nearbyTimer.Stop();
            _nearbyTimer.Dispose();

            _ble.DeviceDiscovered -= AxLEDeviceDiscovered;
            _ble.DeviceAdvertised -= AxLEDeviceAdvertised;
            _ble.DeviceDisconnected -= AxLEDeviceDisconnected;
            _ble.DeviceConnectionLost -= AxLEDeviceDisconnected;

            _ble.DeviceDiscovered -= BootloaderDeviceDiscovered;
            _ble.DeviceDisconnected -= BootloaderDeviceDisconnected;
            _ble.DeviceConnectionLost -= BootloaderDeviceConnectionLost;
        }
    }
}