using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class ConfirmUserInteraction : AxLECommand<bool>
    {
        private bool _confirm = false;

        public ConfirmUserInteraction(float timeout) : base(timeout) { }

        public override async Task SendCommand()
        {
            await Device.TxUart("C");
        }

        protected override bool LookForEnd()
        {
            var ds = string.Join("", Data.ToArray());

            var am = new Regex(@"Confirmed");
            var rm = new Regex(@"!");

            var matches = am.Matches(ds);
            if (matches.Count > 0)
            {
                _confirm = true;

                return true;
            }

            matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                _confirm = false;

                return true;
            }

            return false;
        }

        protected override bool ProcessResult()
        {
            return _confirm;
        }
    }
}
