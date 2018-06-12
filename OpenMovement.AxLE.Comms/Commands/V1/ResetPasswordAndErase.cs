using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class ResetPasswordAndErase : AxLECommandNoResponse
    {
        private readonly string _masterPassword;

        public ResetPasswordAndErase(string masterPassword)
        {
            _masterPassword = masterPassword;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"E{_masterPassword}");
        }
    }
}
