using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriodOffset : AxLECommandNoResponse
    {
        private readonly ulong _offset;

        public WriteGoalPeriodOffset(ulong offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GO{_offset.ToString("X")}");
        }
    }
}
