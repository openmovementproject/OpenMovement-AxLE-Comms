using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Values;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class WriteCurrentBlock : AxLECommand<BlockDetails>
    {
        private readonly UInt16 _blockNo;
        private readonly UInt16 _index;
        private string _match;

        public WriteCurrentBlock(UInt16 blockNo)
        {
            _blockNo = blockNo;
            _index = (UInt16) (_blockNo % 128); // TODO: Make passed config from device
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"W{_index.ToString("X")}");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"T:\d+\r?\nB:\d+\r?\nN:\d+\r?\nE:\d+\r?\nC:\d+\r?\nI:\d+\r?\n";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override BlockDetails ProcessResult()
        {
            var values = _match.Split('T', 'B', 'N', 'E', 'C', 'I', ':', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return new BlockDetails
            {
                Time = UInt32.Parse(values[0]),
                ActiveBlock = UInt16.Parse(values[1]),
                ActiveSamples = UInt16.Parse(values[2]),
                ActiveEpoch = UInt32.Parse(values[3]),
                BlockCount = UInt16.Parse(values[4]),
                DownloadBlock = UInt16.Parse(values[5])
            };
        }
    }
}
