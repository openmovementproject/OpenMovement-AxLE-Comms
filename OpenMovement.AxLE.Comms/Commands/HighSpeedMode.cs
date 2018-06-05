using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class HighSpeedMode : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("F");
        }
    }
}
