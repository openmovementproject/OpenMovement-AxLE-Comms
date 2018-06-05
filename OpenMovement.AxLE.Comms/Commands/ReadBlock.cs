using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenMovement.AxLE.Service.Models;

namespace OpenMovement.AxLE.Comms.Commands
{
    public class ReadBlock : AxLECommand<EpochBlock>
    {
        private const int BlockSize = 512;
        private const int EpochSampleSize = 8;

        private byte[] BlockBytes { get; set; }

        public override async Task SendCommand()
        {
            await Device.TxUart("R");
        }

        protected override bool LookForEnd()
        {
            // Check for end
            var regex = @"[a-fA-F0-9]*\r?\n";

            var rm = new Regex(regex);
            var matches = rm.Matches(Data.Last());
            if (matches.Count == 0)
            {
                return false;
            }

            // Match entire block and process
            var ds = string.Join("", Data.ToArray());

            regex = @"[a-fA-F0-9]+\r?\n";

            rm = new Regex(regex);
            matches = rm.Matches(ds);
            if (matches.Count > 0)
            {
                var match = matches[0].Value;
                var end = @"\r?\n";
                var endM = new Regex(end);

                var endMatch = endM.Matches(match)[0];
                var hexString = match.Substring(0, endMatch.Index);

                BlockBytes = AxLEHelper.StringToByteArray(hexString);

                return BlockBytes.Length == 512;
            }

            return false;
        }

        protected override EpochBlock ProcessResult()
        {
            var data = BlockBytes;

            var metaData = new byte[20];
            Array.Copy(data, 10, metaData, 0, metaData.Length);

            var block = new EpochBlock
            {
                BlockInfo = new EpochBlockInfo
                {
                    BlockNumber = BitConverter.ToUInt16(data, 0),
                    DataLength = BitConverter.ToUInt16(data, 2),
                    DeviceTimestamp = BitConverter.ToUInt32(data, 4)
                },
                BlockFormat = BitConverter.ToUInt16(data, 8),
                MetaData = metaData,
                CRC = BitConverter.ToUInt16(data, data.Length - 2),
                Raw = data
            };

            var epochBytes = new byte[EpochSampleSize];

            var epochSamples = new List<EpochSample>();

            for (var i = 30; i < data.Length && epochSamples.Count < block.BlockInfo.DataLength; i += EpochSampleSize)
            {
                Array.Copy(data, i, epochBytes, 0, epochBytes.Length);
                epochSamples.Add(new EpochSample
                {
                    Battery = (sbyte) epochBytes[0],
                    Temperatue = (sbyte) epochBytes[1],
                    Acceleration = (sbyte) epochBytes[2],
                    Steps = (sbyte) epochBytes[3],
                    Epoch = new sbyte[] { (sbyte) epochBytes[4], (sbyte) epochBytes[5], (sbyte) epochBytes[6], (sbyte) epochBytes[7] }
                });
            }

            block.Samples = epochSamples.ToArray();

            return block;
        }
    }
}
