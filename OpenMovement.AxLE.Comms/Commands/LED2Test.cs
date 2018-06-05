using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class LED2Test : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("2");
        }
    }
}
