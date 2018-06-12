using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class SycroniseEpochTiming : AxLECommandNoResponse
    {
        private readonly UInt16 _offset;

        public SycroniseEpochTiming(UInt16 offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"S{AxLEHelper.ShortToHexWordsLE(_offset)}");
        }
    }
}
