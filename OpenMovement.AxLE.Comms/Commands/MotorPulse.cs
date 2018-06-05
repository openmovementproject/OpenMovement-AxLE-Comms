using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class MotorPulse : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("M");
        }
    }
}
