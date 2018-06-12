using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Values;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class QueryBlockDetails : AxLECommand<BlockDetails>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("Q");
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
