using System;
using System.Threading;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Bluetooth.Values;

namespace OpenMovement.AxLE.Comms.Bluetooth.Interfaces
{
    public interface IBluetoothManager : IDisposable
    {
        event EventHandler<IDevice> DeviceDiscovered;
        event EventHandler<IDevice> DeviceAdvertised;
        event EventHandler<IDevice> DeviceDisconnected;
        event EventHandler<IDevice> DeviceConnectionLost;

        event EventHandler<BluetoothState> StateChanged;
        event EventHandler ScanTimeoutElapsed;

        BluetoothScanMode ScanMode { get; set; }
        int ScanTimeout { get; set; }

        Task StartScan(Guid[] serviceGuids, Func<IDevice, bool> filter);
        Task StopScan();

        Task ConnectToDevice(IDevice device);
        Task<IDevice> ConnectToKnownDevice(string deviceId, CancellationToken ct);
        Task DisconnectDevice(IDevice device);
    }
}
