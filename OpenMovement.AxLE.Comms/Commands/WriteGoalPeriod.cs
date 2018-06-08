using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriod : AxLECommandNoResponse
    {
        private readonly UInt32 _period;

        public WriteGoalPeriod(UInt32 period)
        {
            _period = period;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GP{_period}");
        }
    }
}
