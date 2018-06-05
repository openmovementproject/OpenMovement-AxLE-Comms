using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteConnectionInterval : AxLECommandNoResponse
    {
        private readonly ulong _interval;

        public WriteConnectionInterval(ulong interval)
        {
            _interval = interval;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"V{_interval.ToString("X")}");
        }
    }
}
