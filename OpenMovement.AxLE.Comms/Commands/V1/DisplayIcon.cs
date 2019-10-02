using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class DisplayIcon : AxLECommandNoResponse
    {
        private readonly int _offset;        // Byte offset of image data, 0xffff = disable icon
        private readonly int _start;           // Row on screen to start display, default 64
        private readonly int _height;          // Number of rows on screen, default 32

        // OIoooosshh
        //   oooo = byte offset of image data
        //       ss = start line of image (0-127)
        //         hh = height of image (0-127)

        public DisplayIcon(int offset, int start, int height)    // width = 4 vertical rows of 8 pixels = 32
        {
            _offset = offset;
            _start = start;
            _height = height;
        }

        public DisplayIcon(int offset)
        {
            _offset = offset;
            _start = -1;
            _height = -1;
        }

        public override async Task SendCommand()
        {
            string cmd;
            if (_start >= 0 && _height >= 0)
            {
                cmd = $"OI{AxLEHelper.ShortToHexWordsLE((ushort)_offset)}{AxLEHelper.ByteToHex((byte)_start)}{AxLEHelper.ByteToHex((byte)_height)}";
            }
            else
            {
                cmd = $"OI{AxLEHelper.ShortToHexWordsLE((ushort)_offset)}";
            }
            await Device.TxUart(cmd);
        }
    }
}
