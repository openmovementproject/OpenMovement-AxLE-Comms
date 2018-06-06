using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteGoalThreshold : AxLECommandNoResponse
    {
        private readonly UInt16 _threshold;

        public WriteGoalThreshold(UInt16 threshold)
        {
            _threshold = threshold;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GG{AxLEHelper.ShortToHexWordsLE(_threshold)}");
        }
    }
}
