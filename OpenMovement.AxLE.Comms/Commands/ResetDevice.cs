using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ResetDevice : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("X!");
        }
    }
}
