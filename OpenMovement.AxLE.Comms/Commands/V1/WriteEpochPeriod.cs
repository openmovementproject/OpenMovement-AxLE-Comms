using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteEpochPeriod : AxLECommandNoResponse
    {
        private readonly UInt32 _epochPeriod;

        public WriteEpochPeriod(UInt32 epochPeriod)
        {
            _epochPeriod = epochPeriod;
        }

        public override async Task SendCommand()
        {
			await Device.TxUart($"N{_epochPeriod:X2}");
        }
    }
}
