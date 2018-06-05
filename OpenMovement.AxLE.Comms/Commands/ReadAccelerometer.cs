using System;
using System.Threading.Tasks;
using System.Linq;
using OpenMovement.AxLE.Service.Values;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ReadAccelerometer : AxLECommand<Vector3>
    {
        public override async Task SendCommand()
        {
            await Device.TxUart("A");
        }

        protected override bool LookForEnd()
        {
            return false; // Wait for timeout.
        }

        protected override Vector3 ProcessResult()
        {
            var data = string.Join("", Data.ToArray());
            var vector = new Vector3();

            // TODO: Testing...
            Console.WriteLine(data);

            return vector;
        }
    }
}
