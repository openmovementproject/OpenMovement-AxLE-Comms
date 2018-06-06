using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteCueingPeriod : AxLECommandNoResponse
    {
        private readonly UInt16 _cueingPeriod;

        public WriteCueingPeriod(UInt16 cueingPeriod)
        {
            _cueingPeriod = cueingPeriod;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"C{AxLEHelper.ShortToHexWordsLE(_cueingPeriod)}");
        }
    }
}
