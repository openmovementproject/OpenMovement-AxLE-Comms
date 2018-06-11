using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteCueingPeriod : AxLECommandNoResponse
    {
        private readonly UInt32 _cueingPeriod;

        public WriteCueingPeriod(UInt32 cueingPeriod)
        {
            _cueingPeriod = cueingPeriod;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"C{_cueingPeriod}");
        }
    }
}
