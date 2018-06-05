using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using OpenMovement.AxLE.Comms.Exceptions;

namespace OpenMovement.AxLE.Comms.Interfaces
{
    public interface IAxLEManager : IDisposable
    {
        /// <summary>
        /// Gets or sets Min RSSI for devices to be considered found.
        /// </summary>
        /// <value>The RSSI value in decibels (lower is closer).</value>
        int RssiFilter { get; set; }

        /// <summary>
        /// Occurs when AxLE device is found.
        /// </summary>
        event EventHandler<string> DeviceFound;
        /// <summary>
        /// Occurs when AxLE device is lost (nearby timer expired).
        /// </summary>
        event EventHandler<string> DeviceLost;
        /// <summary>
        /// Occurs when AxLE device is disconnected or communication with it is lost.
        /// </summary>
        event EventHandler<string> DeviceDisconnected;

        /// <summary>
        /// Occurs when a Bootloader device is found.
        /// </summary>
        event EventHandler<IDevice> BootDeviceFound;
        /// <summary>
        /// FUTURE USE.
        /// </summary>
        event EventHandler<IDevice> BootDeviceLost;
        /// <summary>
        /// Occurs when a Bootloader device is disconnected.
        /// </summary>
        event EventHandler<IDevice> BootDeviceDisconnected;

        /// <summary>
        /// Start scanning for AxLE devices. Operation continues until stopped and will not return.
        /// </summary>
        Task StartScan();
        /// <summary>
        /// Start scanning for devices in Bootloader mode.
        /// </summary>
        Task ScanBootloader();
        /// <summary>
        /// Stops scanning for devices.
        /// </summary>
		Task StopScan();

        /// <summary>
        /// Switches to high power scan. MUST STOP AND START SCANNING TO TAKE EFFECT.
        /// </summary>
        void SwitchToHighPowerScan();
        /// <summary>
        /// Switches to low power scan. MUST STOP AND START SCANNING TO TAKE EFFECT.
        /// </summary>
        void SwitchToLowPowerScan();

        /// <summary>
        /// Switch AxLE manager to Bootloader mode for updating AxLE devices.
        /// </summary>
        Task SwitchToBootloaderMode();
        /// <summary>
        /// Switch AxLE manager back to normal mode, looking for AxLE devices.
        /// </summary>
        Task SwitchToNormalMode();

        /// <summary>
        /// Connect to AxLE device. Device will no longer advertise when connected.
        /// </summary>
        /// <exception cref="DeviceNotInRangeException">Thrown when attempting to connect to a device that has not been discovered by the manager.</exception>
        /// <returns>AxLE device.</returns>
        /// <param name="serial">Serial number of device.</param>
        Task<AxLE> ConnectDevice(string serial);
        /// <summary>
        /// Connects to a known AxLE device. Must know DeviceId from previous connection with this device.
        /// </summary>
        /// <returns>AxLE device.</returns>
        /// <param name="serial">Serial number of device.</param>
        /// <param name="timeout">If set to <c>true</c> connection operation will timeout. iOS will look for device forever, Android will produce GATT ERROR if not in range.</param>
        Task<AxLE> ConnectToKnownDevice(string serial, bool timeout = true);
        /// <summary>
        /// Disconnects from AxLE device and disposes of AxLE resources.
        /// </summary>
        /// <param name="device">AxLE device to disconnect.</param>
        Task DisconnectDevice(AxLE device);
    }
}
