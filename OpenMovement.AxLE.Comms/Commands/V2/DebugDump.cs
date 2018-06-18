using System.Threading.Tasks;
using System.Linq;

namespace OpenMovement.AxLE.Comms.Commands.V2
{
    public class DebugDump : AxLECommand<string>
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("?");
        }

        protected override bool LookForEnd()
        {
			return Data.Count >= 11; // Wait until timeout
        }

        protected override string ProcessResult()
        {
            return string.Join("", Data.ToArray());
        }
    }
}
