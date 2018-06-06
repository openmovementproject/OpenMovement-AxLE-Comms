using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriodOffset : AxLECommandNoResponse
    {
        private readonly UInt16 _offset;

        public WriteGoalPeriodOffset(UInt16 offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GO{AxLEHelper.ShortToHexWordsLE(_offset)}");
        }
    }
}
