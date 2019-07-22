using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class WriteRealTime : AxLECommandNoResponse
    {
        private readonly DateTime _time;

        public WriteRealTime(DateTime time)
        {
            _time = time;
        }

        protected override bool LookForEnd()
        {
            bool acknowledged = false;
            while (Data.Count > 0)
            {
                if (Data[0].Trim().StartsWith("T:")) acknowledged = true;
                Data.RemoveAt(0);
            }
            return acknowledged;
        }

        public override async Task SendCommand()
        {
            var timeString = _time.ToString("yy-MM-dd HH:mm:ss");
            await Device.TxUart("T$" + timeString);
        }
    }
}
