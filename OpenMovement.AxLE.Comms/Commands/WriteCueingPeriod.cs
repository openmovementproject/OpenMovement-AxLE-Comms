using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteCueingPeriod : AxLECommandNoResponse
    {
        private readonly ulong _cueingPeriod;

        public WriteCueingPeriod(ulong cueingPeriod)
        {
            _cueingPeriod = cueingPeriod;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"C{_cueingPeriod.ToString("X")}");
        }
    }
}
