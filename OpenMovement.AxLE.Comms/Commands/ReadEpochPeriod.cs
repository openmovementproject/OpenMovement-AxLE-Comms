using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ReadEpochPeriod : AxLECommand<UInt16>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("N?");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"N: *\d+";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override UInt16 ProcessResult()
        {
            var values = _match.Split('N', ':', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return UInt16.Parse(values[0]);
        }
    }
}
