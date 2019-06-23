using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class DisplayIcon : AxLECommandNoResponse
    {
        private readonly UInt16 _offset;

        public DisplayIcon(UInt16 offset)
        {
            _offset = offset;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"OI{AxLEHelper.ShortToHexWordsLE(_offset)}");
        }
    }
}
