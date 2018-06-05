using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ReadConnectionInterval : AxLECommand<ulong>
    {
        private string _match;

        public override async Task SendCommand()
        {
            await Device.TxUart("V");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var regex = @"V: *\d+ms";

            var rm = new Regex(regex);
            var matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _match = matches[0].Value;

                return true;
            }

            return false;
        }

        protected override ulong ProcessResult()
        {
            var values = _match.Split('V', ':', 'm', 's', '\r', '\n').Where(v => !string.IsNullOrEmpty(v)).ToArray();

            return ulong.Parse(values[0]);
        }
    }
}
