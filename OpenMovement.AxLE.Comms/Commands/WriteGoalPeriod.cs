using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriod : AxLECommandNoResponse
    {
        private readonly UInt16 _period;

        public WriteGoalPeriod(UInt16 period)
        {
            _period = period;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GP{AxLEHelper.ShortToHexWordsLE(_period)}");
        }
    }
}
