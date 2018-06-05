using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class PauseLogger : AxLECommandNoResponse
    {
        private readonly ulong _seconds;

        public PauseLogger(ulong seconds)
        {
            _seconds = seconds;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"X{_seconds.ToString("X")}");
        }
    }
}
