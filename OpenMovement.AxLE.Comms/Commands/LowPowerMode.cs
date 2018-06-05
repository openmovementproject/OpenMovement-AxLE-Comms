using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class LowPowerMode : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("L");
        }
    }
}
