using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class LED3Test : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("3");
        }
    }
}
