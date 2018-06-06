using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Values;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class QueryGoalConfig : AxLECommand<GoalConfig>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("G?");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());
            var regex = @"O:\d+\r?\nP:\d+\r?\nG:\d+";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override GoalConfig ProcessResult()
        {
            var values = _match.Split('O', 'P', 'G', ':', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return new GoalConfig
            {
                GoalPeriodOffset = UInt16.Parse(values[0]),
                GoalPeriod = UInt16.Parse(values[1]),
                GoalThreshold = UInt16.Parse(values[2])
            };
        }
    }
}
