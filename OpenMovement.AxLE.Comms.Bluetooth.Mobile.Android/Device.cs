using Android.Bluetooth;

namespace OpenMovement.AxLE.Comms.Bluetooth.Mobile.Android
{
    public class Device : Mobile.Device
    {
        public Device(Plugin.BLE.Abstractions.Contracts.IDevice device) : base(device, ((BluetoothDevice)device.NativeDevice).Address.Replace(":", string.Empty).ToUpper()) {}
    }
}