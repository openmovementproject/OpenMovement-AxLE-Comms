using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteBitmap : AxLECommandNoResponse
    {
        List<string> commandStrings = new List<string>();

        public WriteBitmap(byte[] data, int offset = 0)
        {
            //commandStrings.Add($"OIFFFF");  // Temporarily disable icon (in case of interruption)
            if (offset < 0 || offset % 8 != 0)
            {
                throw new ArgumentException("Offset must be a positive multiple of 8");
            }
            if (data.Length % 8 != 0)
            {
                throw new ArgumentException("Data length must be a multiple of 8");
            }
            for (var i = 0; i < data.Length; i += 8)
            {
                commandStrings.Add($"OW{(offset + i) / 8:X2}{AxLEHelper.ByteArrayToString(data, i, 8)}");
            }
            //commandStrings.Add($"OI{AxLEHelper.ShortToHexWordsLE((ushort)offset)}");
        }

        public WriteBitmap(string file)
            : this(File.ReadAllBytes(file), 0)
        { }

        // Write next sub-command, returns true if written, false if no more to write
        private bool WriteNext()
        {
            if (commandStrings.Count == 0) return false;
            Device.TxUart(commandStrings[0]);
            commandStrings.RemoveAt(0);
            return true;
        }

        protected override bool LookForEnd()
        {
            bool acknowledged = false;
            while (Data.Count > 0)
            {
                if (Data[0].Trim().StartsWith("O+W")) acknowledged = true;
                Data.RemoveAt(0);
            }
            if (!acknowledged) return false;
            return !WriteNext();
        }

        public override async Task SendCommand()
        {
            WriteNext();
            await Task.Delay(0);
            /*
            foreach (var command in commandStrings)
            {
                await Device.TxUart(command);
                await Task.Delay(200);
            }
            */
        }
    }
}
