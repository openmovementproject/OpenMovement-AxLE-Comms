using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class MotorTest : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("1");
        }
    }
}
