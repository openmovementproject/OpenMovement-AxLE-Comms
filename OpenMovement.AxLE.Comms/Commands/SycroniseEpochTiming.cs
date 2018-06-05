using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class SycroniseEpochTiming : AxLECommandNoResponse
    {
        private readonly ulong _offset;

        public SycroniseEpochTiming(ulong offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"S{_offset.ToString("X")}");
        }
    }
}
