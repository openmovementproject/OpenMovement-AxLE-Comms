using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class HardwareControl : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("Y");
        }
    }
}
