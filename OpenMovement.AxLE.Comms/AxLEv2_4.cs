using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands.V1;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv2_4 : AxLEv2_3
    {
        public AxLEv2_4(IAxLEDevice device, string serial) : base(device, serial) { }

        public override async Task WriteBitmap(string file)
        {
            await _processor.AddCommand(new WriteBitmap(file));
        }

        public override async Task WriteBitmap(byte[] data, int offset = 0)
        {
            await _processor.AddCommand(new WriteBitmap(data, offset));
        }

        public override async Task ClearDisplay()
        {
            await _processor.AddCommand(new ClearDisplay());
        }

        public override async Task DisplayIcon(ushort offset)
        {
            await _processor.AddCommand(new DisplayIcon(offset));
        }

        public override async Task PaintDisplay(ushort offset, byte startCol, byte startRow, byte cols, byte rows, byte span)
        {
            await _processor.AddCommand(new PaintDisplay(offset, startCol, startRow, cols, rows, span));
        }

        public override async Task WriteRealTime(DateTime time)
        {
            await _processor.AddCommand(new WriteRealTime(time));
        }
    }
}
