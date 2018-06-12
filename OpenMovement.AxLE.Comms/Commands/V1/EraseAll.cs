using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class EraseAll : AxLECommandNoResponse
    {
        private readonly bool _destructive;

        public EraseAll(bool destructive)
        {
            _destructive = destructive;
        }

        public override async Task SendCommand()
        {
            if (_destructive)
            {
                await Device.TxUart("E!");
            }
            else
            {
                await Device.TxUart("E");
            }
        }
    }
}
