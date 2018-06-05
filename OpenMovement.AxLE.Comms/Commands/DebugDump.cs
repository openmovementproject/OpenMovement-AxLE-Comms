using System.Threading.Tasks;
using System.Linq;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class DebugDump : AxLECommand<string>
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("D");
        }

        protected override bool LookForEnd()
        {
            return false; // Wait until timeout
        }

        protected override string ProcessResult()
        {
            return string.Join("", Data.ToArray());
        }
    }
}
