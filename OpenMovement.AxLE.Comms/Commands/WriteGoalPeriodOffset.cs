using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriodOffset : AxLECommandNoResponse
    {
        private readonly UInt32 _offset;

        public WriteGoalPeriodOffset(UInt32 offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GO{_offset}");
        }
    }
}
