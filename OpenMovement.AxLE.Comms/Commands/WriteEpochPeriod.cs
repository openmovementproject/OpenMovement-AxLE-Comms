using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteEpochPeriod : AxLECommandNoResponse
    {
        private readonly ulong _epochPeriod;

        public WriteEpochPeriod(ulong epochPeriod)
        {
            _epochPeriod = epochPeriod;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"N{_epochPeriod.ToString("X")}");
        }
    }
}
