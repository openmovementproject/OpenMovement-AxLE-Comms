using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class PaintDisplay : AxLECommandNoResponse
    {
        private readonly UInt16 _offset;
        private readonly byte _startCol;
        private readonly byte _startRow;
        private readonly byte _cols;
        private readonly byte _rows;
        private readonly byte _span;

        public PaintDisplay(UInt16 offset, byte startCol, byte startRow, byte cols, byte rows, byte span)
        {
            _offset = offset;
            _startCol = startCol;
            _startRow = startRow;
            _cols = cols;
            _rows = rows;
            _span = span;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"OD{AxLEHelper.ShortToHexWordsLE(_offset)}{_startCol:XX}{_startRow:XX}{_cols:XX}{_rows:XX}{_span:XX}");
        }
    }
}
