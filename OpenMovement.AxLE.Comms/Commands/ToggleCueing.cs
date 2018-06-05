using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ToggleCueing : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("C");
        }
    }
}
