using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteGoalThreshold : AxLECommandNoResponse
    {
        private readonly UInt32 _threshold;

        public WriteGoalThreshold(UInt32 threshold)
        {
            _threshold = threshold;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"GG{_threshold}");
        }
    }
}
