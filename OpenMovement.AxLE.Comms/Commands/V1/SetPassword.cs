using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class SetPassword : AxLECommandNoResponse
    {
        private readonly string _password;

        public SetPassword(string password)
        {
            _password = password;
        }

        public override async Task SendCommand()
        {
            await Device.TxUart($"P{_password}");
        }
    }
}
