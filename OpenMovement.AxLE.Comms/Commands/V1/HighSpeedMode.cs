using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class HighSpeedMode : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("F");
        }
    }
}
