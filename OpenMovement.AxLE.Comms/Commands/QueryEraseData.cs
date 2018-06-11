using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenMovement.AxLE.Comms.Values;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class QueryEraseData : AxLECommand<EraseData>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("E?");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"B: *\d+\r?\nR: *\d+\r?\nE: *\d+";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override EraseData ProcessResult()
        {
            var values = _match.Split('B', 'R', 'E', ':', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return new EraseData
            {
                BatteryCycles = int.Parse(values[0]),
                ResetCycles = UInt32.Parse(values[1]),
                EraseCycles = UInt32.Parse(values[2])
            };
        }
    }
}
