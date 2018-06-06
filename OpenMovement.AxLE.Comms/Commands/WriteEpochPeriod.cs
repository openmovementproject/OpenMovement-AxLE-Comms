using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteEpochPeriod : AxLECommandNoResponse
    {
        private readonly UInt16 _epochPeriod;

        public WriteEpochPeriod(UInt16 epochPeriod)
        {
            _epochPeriod = epochPeriod;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"N{AxLEHelper.ShortToHexWordsLE(_epochPeriod)}");
        }
    }
}
