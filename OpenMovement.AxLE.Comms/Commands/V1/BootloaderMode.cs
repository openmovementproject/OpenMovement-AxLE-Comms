using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class BootloaderMode : AxLECommandNoResponse
    {
        private readonly AxLEManager _manager;

        public BootloaderMode(AxLEManager manager)
        {
            _manager = manager;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart("XD");

            // Wait for Bootloader to come up
            _manager.BootDeviceFound += async (sender, e) => 
            {
                // Manipulate or whatever...

                await _manager.SwitchToNormalMode();
            };

            await _manager.SwitchToBootloaderMode();
        }
    }
}
