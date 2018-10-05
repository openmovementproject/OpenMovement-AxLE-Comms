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
                var endIndex = endMatch.Index;

                var nibbleCount = endIndex;
                if ((nibbleCount & 1) != 0)
                {
                    Console.WriteLine($"WARNING: Packet contained odd number of nibbles, ignoring last: {nibbleCount}");
                    nibbleCount &= ~1;
                }
                var hexString = match.Value.Substring(0, nibbleCount);

                LastBlockBytes = AxLEHelper.StringToByteArray(hexString);

                Data.Clear();

                // Consume line ending
                if (ds[endIndex] == '\r') { endIndex++; }
                if (ds[endIndex] == '\n') { endIndex++; }

                // Keep any residual characters for next packet
                if (endIndex < ds.Length)
                {
                    Data.Add(ds.Substring(endIndex));
                }

                return true;
            }

            return false;
        }

        protected override AccBlock ProcessBlock()
        {
            var block = new AccBlock();

            if (LastBlockBytes.Length >= 4)
            {
                block.Timestamp = BitConverter.ToUInt32(LastBlockBytes, 0);
            }
            if (LastBlockBytes.Length >= 6)
            {
                block.Battery = BitConverter.ToUInt16(LastBlockBytes, 4);
            }
            if (LastBlockBytes.Length >= 8)
            {
                block.Temperature = BitConverter.ToUInt16(LastBlockBytes, 6);
            };

            var samples = new List<Int16[]>();

            if (LastBlockBytes.Length < 8)
            {
                Console.WriteLine($"WARNING: Packet too short: ${LastBlockBytes.Length}");
            }
            else
            {
                const int bytesPerSample = 6;
                var sampleBytes = new byte[bytesPerSample];

                if (((LastBlockBytes.Length - 8) % bytesPerSample) != 0)
                {
                    Console.WriteLine($"WARNING: Packet contains a partial sample, ignoring: {LastBlockBytes.Length}");
                }

                var maxSamples = (LastBlockBytes.Length - 8) / bytesPerSample;
                if (maxSamples != SampleCount)
                {
                    Console.WriteLine($"WARNING: Packet sample count different to expected: {maxSamples}, {SampleCount}");
                }

                if (maxSamples > SampleCount) maxSamples = SampleCount;
                for (int i = 0; i < maxSamples; i++)
                {
                    Array.Copy(LastBlockBytes, 8 + bytesPerSample * i, sampleBytes, 0, sampleBytes.Length);
                    var sample = new Int16[3];

                    sample[0] = BitConverter.ToInt16(sampleBytes, 0);
                    sample[1] = BitConverter.ToInt16(sampleBytes, 2);
                    sample[2] = BitConverter.ToInt16(sampleBytes, 4);

                    samples.Add(sample);
                }
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
