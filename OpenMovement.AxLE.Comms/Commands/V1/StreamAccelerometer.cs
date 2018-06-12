using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenMovement.AxLE.Comms.Values;
using System.Collections.Generic;

namespace OpenMovement.AxLE.Comms.Commands.V1
{
    public class StreamAccelerometer : AxLECommandStream<AccBlock>
    {
        const int SampleCount = 25;

        private byte[] LastBlockBytes { get; set; }

        public override async Task SendStartCommand()
        {
            await Device.TxUart("I");
        }

        protected override bool LookForBlock()
        {
            var opm = new Regex(@"(OP: *01\r?\n)+");
            var rm = new Regex(@"[a-fA-F0-9]+\r?\n");

            var ds = string.Join("", Data.ToArray());

            if (opm.Match(ds).Success)
            {
                Data.Clear();
                return false;
            }

            var match = rm.Match(ds);
            if (match.Success)
            {
                var end = @"\r?\n";
                var endM = new Regex(end);

                var endMatch = endM.Match(match.Value);
                var hexString = match.Value.Substring(0, endMatch.Index);

                Data.Clear();

                LastBlockBytes = AxLEHelper.StringToByteArray(hexString);

                return true;
            }

            return false;
        }

        protected override AccBlock ProcessBlock()
        {
            var block = new AccBlock
            {
                Timestamp = BitConverter.ToUInt32(LastBlockBytes, 0),
                Battery = BitConverter.ToUInt16(LastBlockBytes, 4),
                Temperature = BitConverter.ToUInt16(LastBlockBytes, 6)
            };

            var samples = new List<Int16[]>();
            var sampleBytes = new byte[6];

            for (int i = 8; i < 8 + 6 * SampleCount; i += 6)
            {
                Array.Copy(LastBlockBytes, i, sampleBytes, 0, sampleBytes.Length);
                var sample = new Int16[3];

                sample[0] = BitConverter.ToInt16(sampleBytes, 0);
                sample[1] = BitConverter.ToInt16(sampleBytes, 2);
                sample[2] = BitConverter.ToInt16(sampleBytes, 4);

                samples.Add(sample);
            }

            block.Samples = samples.ToArray();

            return block;
        }

        public override async Task SendStopCommand()
        {
            await Device.TxUart("I");
        }
    }
}
