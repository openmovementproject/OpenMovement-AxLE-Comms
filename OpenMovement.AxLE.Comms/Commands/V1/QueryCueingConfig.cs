using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Values;
using System;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class QueryCueingConfig : AxLECommand<CueingConfig>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("C?");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"Q: *\d+\r?\nC: *\d+";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override CueingConfig ProcessResult()
        {
            var values = _match.Split('Q', 'C', ':', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return new CueingConfig
            {
                Period = UInt32.Parse(values[0]),
                Cueing = UInt16.Parse(values[1]) > 0
            };
        }
    }
}
