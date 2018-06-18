using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands.V1;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Interfaces;
using OpenMovement.AxLE.Comms.Values;
using OpenMovement.AxLE.Service.Models;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv1_5 : IAxLE
    {
		protected readonly IAxLEDevice _device;
		protected readonly IAxLEProcessor _processor;

        public string DeviceId { get; }
        public string SerialNumber { get; }
        public int Battery { get; private set; }
        public UInt32 DeviceTime { get; private set; }
        public EraseData EraseData { get; private set; }

        private UInt32 _connectionInterval;
        public UInt32 ConnectionInterval
        {
            get => _connectionInterval;
            set
            {
                _connectionInterval = value;
                _processor.AddCommand(new WriteConnectionInterval(ConnectionInterval));
            }
        }

        private bool _cueing;
        public bool Cueing
        {
            get => _cueing;
            set
            {
                if (Cueing != value)
                {
                    _cueing = value;
                    _processor.AddCommand(new ToggleCueing());
                }
            }
        }

        private UInt32 _cueingPeriod;
        public UInt32 CueingPeriod
        {
            get => _cueingPeriod;
            set
            {
                _cueingPeriod = value;
                _processor.AddCommand(new WriteCueingPeriod(CueingPeriod));
            }
        }

        private UInt32 _epochPeriod;
        public UInt32 EpochPeriod
        {
            get => _epochPeriod;
            set
            {
                _epochPeriod = value;
                _processor.AddCommand(new WriteEpochPeriod(EpochPeriod));
            }
        }

        private UInt32 _goalPeriodOffset;
        public UInt32 GoalPeriodOffset
        {
            get => _goalPeriodOffset;
            set
            {
                _goalPeriodOffset = value;
                _processor.AddCommand(new WriteGoalPeriodOffset(GoalPeriodOffset));
            }
        }

        private UInt32 _goalPeriod;
        public UInt32 GoalPeriod
        {
            get => _goalPeriod;
            set
            {
                _goalPeriod = value;
                _processor.AddCommand(new WriteGoalPeriod(GoalPeriod));
            }
        }

        private UInt32 _goalThreshold;
        public UInt32 GoalThreshold
        {
            get => _goalThreshold;
            set
            {
                _goalThreshold = value;
                _processor.AddCommand(new WriteGoalThreshold(GoalThreshold));
            }
        }

        private StreamAccelerometer CurrentStreamCommand { get; set; }

        public event EventHandler<AccBlock> AccelerometerStream;

        public AxLEv1_5(IAxLEDevice device, string serial)
        {
            _device = device;
            _processor = new AxLEProcessor(device);

            DeviceId = _device.DeviceId;
            SerialNumber = serial;

            _processor.StartProcessor();
        }

        public async Task<bool> Authenticate(string password)
        {
            return await _processor.AddCommand(new Unlock(password));
        }

        public async Task UpdateDeviceState()
        {
            await ReadBattery();
            await ReadDeviceTime();
            await ReadEraseData();
            await ReadConnectionInterval();
            await ReadCueingStatus();
            await ReadEpochPeriod();
            await ReadGoalConfig();
        }

		public async Task UpdateDeviceState(uint flags)
        {
    		// TODO: Remove this if not needed, or make these enum flags instead
			if ((flags & 0x0001) != 0) await ReadBattery();
			if ((flags & 0x0002) != 0) await ReadDeviceTime();
			if ((flags & 0x0004) != 0) await ReadEraseData();
			if ((flags & 0x0008) != 0) await ReadConnectionInterval();
			if ((flags & 0x0010) != 0) await ReadCueingStatus();
			if ((flags & 0x0020) != 0) await ReadEpochPeriod();
			if ((flags & 0x0040) != 0) await ReadGoalConfig();
		}

        public async Task SetPassword(string password)
        {
            await _processor.AddCommand(new SetPassword(password));
        }

        public async Task ResetPassword()
        {
            await _processor.AddCommand(new ResetPasswordAndErase(SerialNumber.Substring(SerialNumber.Length - 6)));
        }

        public async Task VibrateDevice()
        {
            await _processor.AddCommand(new MotorPulse());
        }

        public async Task LEDFlash()
        {
            for (var i = 0; i < 5; i++)
            {
                await _processor.AddCommand(new LED2Test());
                Thread.Sleep(100);
                await _processor.AddCommand(new AllHardwareOff());
                await _processor.AddCommand(new LED3Test());
                Thread.Sleep(100);
                await _processor.AddCommand(new AllHardwareOff());
            }
        }

        public async Task StartAccelerometerStream()
        {
            if (CurrentStreamCommand == null)
            {
                CurrentStreamCommand = new StreamAccelerometer();
                CurrentStreamCommand.NewBlock += (sender, e) =>
                {
                    AccelerometerStream?.Invoke(this, e);
                };
                await _processor.AddCommand(CurrentStreamCommand);
            }
        }

        public async Task StopAccelerometerStream()
        {
            if (CurrentStreamCommand != null)
            {
                await CurrentStreamCommand.StopStream();
                CurrentStreamCommand = null;
            }
        }

        public async Task<EpochBlock[]> SyncEpochData(UInt16 lastBlock, UInt32? lastRtc = null, DateTimeOffset? lastSync = null)
        {
            var blockDetails = await _processor.AddCommand(new QueryBlockDetails());
            UInt16 readFrom = lastBlock;
            UInt16 readTo = blockDetails.ActiveBlock;
            // Check if we've lost data since the last sync (device's circular buffer has wrapped)
            if (((UInt16)(readTo - readFrom)) > AxLEConfig.BlockCount)
            {
                // Adjust to read as much as we can
                UInt16 newReadFrom = (UInt16)(readTo - AxLEConfig.BlockCount + 1);
                Console.WriteLine($"WARNING: Some data was lost from the device through the circular buffer was asking to read from {readFrom} to {readTo}, will now read from {newReadFrom}.");
                readFrom = newReadFrom;
            }
            return await SyncEpochData(readFrom, readTo, lastRtc, lastSync);
        }

        public async Task<EpochBlock[]> SyncEpochData(UInt16 readFrom, UInt16 readTo, UInt32? startRtc = null, DateTimeOffset? startTime = null)
        {
			if (((UInt16)(readTo - readFrom)) > AxLEConfig.BlockCount)
			{
				throw new InvalidBlockRangeException(readFrom, readTo);
			}

            var blocks = new List<EpochBlock>();

            await _processor.AddCommand(new HighSpeedMode());
            await ReadConnectionInterval();
            
            var blockDetails = await WriteCurrentBlock(readFrom);

            var current = readFrom;
			var count = (UInt16)(readTo - readFrom + 1);    // inclusive range
            if (count > AxLEConfig.BlockCount)
            {
                throw new InvalidBlockRangeException(readFrom, readTo);
            }
			for (var i = 0; i < count; i++)
			{
				EpochBlock block = null;
				try
				{
					block = await SyncCurrentEpochBlock();
				}
				catch (Exception e)
				{
					if (e is BlockSyncFailedException || e is CommandFailedException)
					{
#if DEBUG_COMMS
						Console.WriteLine($"SYNC -- READ BLOCK {current} FAILED -- RESYNCING");
#endif
						// If read operation failure cause by device not writing block in time wait for 3 connection intervals and retry.
						Thread.Sleep((int)(ConnectionInterval * 3));

						await WriteCurrentBlock(current);
                        try
                        {
                            block = await SyncCurrentEpochBlock();
                        }
                        catch (Exception)
                        {
                            throw;  // retry failed
                        }
                    }
                    else
                    {
                        throw;      // another problem
                    }
                }
#if DEBUG_COMMS
				Console.WriteLine($"SYNC -- Read Block: {block.BlockInfo.BlockNumber}");
#endif
				if (current != block.BlockInfo.BlockNumber)
				{
					Console.WriteLine($"WARNING: Unexpected block read, found {block.BlockInfo.BlockNumber} expected {current}.");
				}
				current = (UInt16)(block.BlockInfo.BlockNumber + 1);    // next block

				blocks.Add(block);
			}
				
			if (blocks.Count != count)
			{
				Console.WriteLine($"WARNING: Only read {blocks.Count} blocks, expected {count}.");
			}
#if DEBUG_COMMS
            Console.WriteLine($"SYNC COMPLETE -- Blocks Read: {blocks.Count}");
#endif

            await _processor.AddCommand(new LowPowerMode());

            await ReadDeviceTime();

            return CalculateTimestamps(blocks.ToArray(), startRtc, startTime, DeviceTime, DateTime.UtcNow);
        }

        public async Task<EpochBlock> SyncCurrentEpochBlock()
        {
            var block = await _processor.AddCommand(new ReadBlock());

            // Checks CRC and resyncs once before erroring sync operation
            if (CheckCRC(block.Raw, block.CRC))
            {
                return block;
            }

            throw new BlockSyncFailedException(block.BlockInfo.BlockNumber, block.Raw);
        }

        public async Task<BlockDetails> WriteCurrentBlock(UInt16 blockNo)
        {
            return await _processor.AddCommand(new WriteCurrentBlock(blockNo));
        }

        public async Task<BlockDetails> ReadBlockDetails()
        {
            return await _processor.AddCommand(new QueryBlockDetails());
        }

		public virtual async Task<string> DebugDump()
		{
			return await _processor.AddCommand(new DebugDump());
		}

        private bool CheckCRC(byte[] data, ushort crc)
        {
            if (crc == 0xFFFF) // Most likely Active Block, partial data
                return true;

            short total = 0;
            for (var i = 0; i + 1 < data.Length; i += 2)
            {
                total += (short) ((data[i + 1] << 8) + data[i]);
            }

            return total == 0;
        }

        private EpochBlock[] CalculateTimestamps(EpochBlock[] blocks, UInt32? lastRtc, DateTimeOffset? lastSync, UInt32 currentRtc, DateTimeOffset currentTime)
        {
            if (blocks.Length < 2)
                return CalculateTimestampsForSet(blocks, currentRtc, currentTime);
            
            var sets = new List<EpochBlock[]>();
            var currentSet = new List<EpochBlock>();
            for (var i = blocks.Length - 2; i >= 0; i--)
            {
                var diff = Math.Abs(blocks[i + 1].BlockInfo.DeviceTimestamp - blocks[i].BlockInfo.DeviceTimestamp);
                if (diff > AxLEConfig.BlockTimestampOutOfRangeThresholdFactor * (blocks[i].BlockInfo.EpochPeriod * blocks[i].BlockInfo.DataLength))
                {
                    currentSet.Add(blocks[i + 1]);
                    currentSet.Reverse();
                    sets.Add(currentSet.ToArray());
                    currentSet = new List<EpochBlock>();

#if DEBUG_COMMS
                    Console.WriteLine($"SYNC -- BATTERY FAILURE DETECTED (TIMES FAILED: {sets.Count})");
#endif
                }
                else
                {
                    currentSet.Add(blocks[i + 1]);
                }
            }

            currentSet.Add(blocks[0]);
            currentSet.Reverse();
            sets.Add(currentSet.ToArray());

            if (sets.Count == 1 || !lastRtc.HasValue || !lastSync.HasValue)
            {
                return CalculateTimestampsForSet(sets.First(), currentRtc, currentTime);
            }

            var firstBlock = blocks.First();
            if (lastRtc.HasValue &&
                Math.Abs(firstBlock.BlockInfo.DeviceTimestamp - lastRtc.Value) < firstBlock.BlockInfo.EpochPeriod * AxLEConfig.BlockTimestampOutOfRangeThresholdFactor)
            {
                throw new ArgumentException("LastRTC did not match band data. If the start timestamp is unknown do not pass this parameter.");
            }

            var endSet = sets.First();
            var startSet = sets.Last();

            var offsetRtc = startSet.Last().BlockInfo.DeviceTimestamp;
			var offsetTime = lastSync.Value.AddSeconds((int)(offsetRtc - startSet.First().BlockInfo.DeviceTimestamp));

            var recoveredBlocks = new List<EpochBlock>();
            recoveredBlocks.AddRange(CalculateTimestampsForSet(startSet, offsetRtc, offsetTime));
            recoveredBlocks.AddRange(CalculateTimestampsForSet(endSet, currentRtc, currentTime));

            return recoveredBlocks.ToArray();
        }

        private EpochBlock[] CalculateTimestampsForSet(EpochBlock[] blocks, UInt32 offsetRtc, DateTimeOffset offsetTime)
        {
            foreach (var block in blocks)
            {
                block.BlockInfo.Timestamp = CalculateTimestamp(block.BlockInfo.DeviceTimestamp, offsetRtc, offsetTime);
            }

            return blocks;
        }

        private DateTimeOffset CalculateTimestamp(UInt32 timestamp, UInt32 currentRtc, DateTimeOffset currentTime)
        {
			return currentTime.AddSeconds((int) (timestamp - currentRtc));
        }

        private async Task ReadBattery()
        {
            Battery = await _processor.AddCommand(new ReadBattery());
        }

        private async Task ReadDeviceTime()
        {
            DeviceTime = await _processor.AddCommand(new ReadDeviceTime());
        }

        private async Task ReadEraseData()
        {
            EraseData = await _processor.AddCommand(new QueryEraseData());
        }

        private async Task ReadConnectionInterval()
        {
            _connectionInterval = await _processor.AddCommand(new ReadConnectionInterval());
        }

        private async Task ReadCueingStatus()
        {
            var cueingConfig = await _processor.AddCommand(new QueryCueingConfig());

            _cueing = cueingConfig.Cueing;
            _cueingPeriod = cueingConfig.Period;
        }

        private async Task ReadEpochPeriod()
        {
            _epochPeriod = await _processor.AddCommand(new ReadEpochPeriod());
        }

        private async Task ReadGoalConfig()
        {
            var goalConfig = await _processor.AddCommand(new QueryGoalConfig());

            _goalPeriodOffset = goalConfig.GoalPeriodOffset;
            _goalPeriod = goalConfig.GoalPeriod;
            _goalThreshold = goalConfig.GoalThreshold;
        }

        public void Dispose()
        {
            _processor.Dispose();
            _device.Dispose();
        }
    }
}