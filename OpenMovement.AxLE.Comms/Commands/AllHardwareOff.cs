using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class AllHardwareOff : AxLECommandNoResponse
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("O");
        }
    }
}
