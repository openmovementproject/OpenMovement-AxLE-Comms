using OpenMovement.AxLE.Comms.Bluetooth.Interfaces;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEDevice : IAxLEDevice
    {
        private const double RxProcessInterval = 50;

        private readonly IDevice _device;
        private readonly System.Timers.Timer _rxTimer;
        private readonly ConcurrentQueue<Task> _rxTasks;

        public string DeviceId { get; }
        public double HardwareVersion { get; private set; }
        public double FirmwareVersion { get; private set; }
        public bool Ready { get; private set; }
        public event EventHandler<string> RxUart;

        private ICharacteristic RxCharac { get; set; }
        private ICharacteristic TxCharac { get; set; }

        private bool RxProcessing { get; set; }

        public AxLEDevice(IDevice device)
        {
            _device = device;
            DeviceId = _device.Id;

            _rxTimer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = RxProcessInterval
            };

            _rxTimer.Elapsed += (s, a) =>
            {
                if (!RxProcessing)
                {
                    ProcessRxTasks();
                }
            };

            RxProcessing = false;
            _rxTasks = new ConcurrentQueue<Task>();

            _rxTimer.Start();
        }

        public async Task OpenComms()
        {
            var infoService = await _device.GetService(AxLEUuid.DeviceInformationServiceUuid);
            var hardwareCharac = await infoService.GetCharacteristic(AxLEUuid.HardwareCharacUuid);
            var firmwareCharac = await infoService.GetCharacteristic(AxLEUuid.FirmwareCharacUuid);

            var hardware = Encoding.UTF8.GetString(await hardwareCharac.Read());
            var firmware = Encoding.UTF8.GetString(await firmwareCharac.Read());

            HardwareVersion = double.Parse(hardware);
            FirmwareVersion = double.Parse(firmware);

            var uartService = await _device.GetService(AxLEUuid.UartServiceUuid);

            RxCharac = await uartService.GetCharacteristic(AxLEUuid.UartRxCharacUuid);
            TxCharac = await uartService.GetCharacteristic(AxLEUuid.UartTxCharacUuid);

            RxCharac.ValueUpdated += RxUartData;
            await RxCharac.RegisterForNotifications();

            Ready = true;
        }

        private void RxUartData(object sender, byte[] data)
        {
            var dataString = Encoding.ASCII.GetString(data);
#if DEBUG_COMMS
            Console.WriteLine("DEBUG RX -- " + dataString);
#endif
            _rxTasks.Enqueue(new Task(() =>
            {
                RxUart?.Invoke(this, dataString);
            }));
        }

        public async Task TxUart(string message)
        {
            if (!Ready) throw new DeviceNotReadyException();
#if DEBUG_COMMS
            Console.WriteLine("DEBUG TX -- " + message);
#endif
            await TxCharac.Write(ConvertToUtf8(message));
        }

        private async void ProcessRxTasks()
        {
            if (!RxProcessing)
            {
                RxProcessing = true;

                while (_rxTasks.Count > 0)
                {
                    if (_rxTasks.TryDequeue(out Task task))
                    {
                        task.Start();
                        await task;
                    }
                }

                RxProcessing = false;
            }
        }

        private byte[] ConvertToUtf8(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        public void Dispose()
        {
            RxCharac.ValueUpdated -= RxUartData;
            RxCharac.UnregisterForUpdates();

            _rxTimer.Stop();
            _rxTimer.Dispose();
        }
    }
}
