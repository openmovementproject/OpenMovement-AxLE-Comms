using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteConnectionInterval : AxLECommandNoResponse
    {
        private readonly UInt32 _interval;

        public WriteConnectionInterval(UInt32 interval)
        {
            _interval = interval;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"V{_interval}");
        }
    }
}
