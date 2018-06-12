using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class Unlock : AxLECommand<bool>
    {
        private readonly string _password;
        private bool _success;

        public Unlock(string password)
        {
            _password = password;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"U{_password}");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var am = new Regex(@"Authenticated");
            var rm = new Regex(@"!");

            var matches = am.Matches(ds);
            if (matches.Count > 0)
            {
                _success = true;

                return true;
            }

            matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _success = false;

                return true;
            }

            return false;
        }

        protected override bool ProcessResult()
        {
            return _success;
        }
    }
}
