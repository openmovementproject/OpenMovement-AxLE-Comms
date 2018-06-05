using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OpenMovement.AxLE.Comms.Bluetooth.Mobile.Android
{
    public class BluetoothManager : Mobile.BluetoothManager
    {
        public BluetoothManager(IBluetoothLE ble) : base(ble) {}

        protected override void NativeDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            NativeDeviceDiscovered(new Device(args.Device));
        }

        protected override void NativeDeviceAdvertised(object sender, DeviceEventArgs args)
        {
            NativeDeviceAdvertised(new Device(args.Device));
        }

        protected override void NativeDeviceDisconnected(object sender, DeviceEventArgs args)
        {
            NativeDeviceDisconnected(new Device(args.Device));
        }

        protected override void NativeDeviceConnectionLost(object sender, DeviceEventArgs args)
        {
            NativeDeviceConnectionLost(new Device(args.Device));
        }
    }
}