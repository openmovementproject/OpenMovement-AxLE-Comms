using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class ReadBattery : AxLECommand<int>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("B");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"B: *\d{1,3}%";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override int ProcessResult()
        {
            var values = _match.Split('B', ':', '%', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return int.Parse(values[0]);
        }
    }
}
