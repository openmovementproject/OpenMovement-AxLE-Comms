using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class HardwareControl : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("Y");
        }
    }
}
