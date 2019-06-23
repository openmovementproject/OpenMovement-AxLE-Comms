using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteBitmap : AxLECommandNoResponse
    {
        private readonly string _path;

        public WriteBitmap(string path)
        {
            _path = path;
        }

        public override async Task SendCommand()
        {
            var commandStrings = new List<string>();
            using (var file = new FileStream(_path, FileMode.Open))
            {
                int offset = 0;
                while (file.CanRead)
                {
                    byte[] bytes = new byte[8];
                    offset += file.Read(bytes, offset, 8);

                    commandStrings.Add($"OW{offset / 8:XX}{AxLEHelper.ByteArrayToString(bytes)}");
                }
            }

            foreach (var command in commandStrings)
            {
                await Device.TxUart(command);
                await Task.Delay(100);
            }
        }
    }
}
