using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteConnectionInterval : AxLECommandNoResponse
    {
        private readonly UInt16 _interval;

        public WriteConnectionInterval(UInt16 interval)
        {
            _interval = interval;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"V{AxLEHelper.ShortToHexWordsLE(_interval)}");
        }
    }
}
