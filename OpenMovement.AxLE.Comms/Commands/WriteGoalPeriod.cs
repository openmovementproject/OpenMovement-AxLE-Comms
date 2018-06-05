using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalPeriod : AxLECommandNoResponse
    {
        private readonly ulong _period;

        public WriteGoalPeriod(ulong period)
        {
            _period = period;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GP{_period.ToString("X")}");
        }
    }
}
