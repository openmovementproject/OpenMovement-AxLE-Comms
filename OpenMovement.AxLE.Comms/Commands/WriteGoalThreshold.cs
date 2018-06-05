using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalThreshold : AxLECommandNoResponse
    {
        private readonly ulong _threshold;

        public WriteGoalThreshold(ulong threshold)
        {
            _threshold = threshold;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GG{_threshold.ToString("X")}");
        }
    }
}
