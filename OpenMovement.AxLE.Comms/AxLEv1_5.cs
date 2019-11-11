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

        protected UInt32 _connectionInterval;
        public UInt32 ConnectionInterval
        {
            get => _connectionInterval;
            set
            {
                _connectionInterval = value;
                _processor.AddCommand(new WriteConnectionInterval(ConnectionInterval));
            }
        }

        protected bool _cueing;
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

        protected UInt32 _cueingPeriod;
        public UInt32 CueingPeriod
        {
            get => _cueingPeriod;
            set
            {
                _cueingPeriod = value;
                _processor.AddCommand(new WriteCueingPeriod(CueingPeriod));
            }
        }

        protected UInt32 _epochPeriod;
        public UInt32 EpochPeriod
        {
            get => _epochPeriod;
            set
            {
                _epochPeriod = value;
                _processor.AddCommand(new WriteEpochPeriod(EpochPeriod));
            }
        }

        protected UInt32 _goalPeriodOffset;
        public UInt32 GoalPeriodOffset
        {
            get => _goalPeriodOffset;
            set
            {
                _goalPeriodOffset = value;
                _processor.AddCommand(new WriteGoalPeriodOffset(GoalPeriodOffset));
            }
        }

        protected UInt32 _goalPeriod;
        public UInt32 GoalPeriod
        {
            get => _goalPeriod;
            set
            {
                _goalPeriod = value;
                _processor.AddCommand(new WriteGoalPeriod(GoalPeriod));
            }
        }

        protected UInt32 _goalThreshold;
        public UInt32 GoalThreshold
        {
            get => _goalThreshold;
            set
            {
                _goalThreshold = value;
                _processor.AddCommand(new WriteGoalThreshold(GoalThreshold));
            }
        }

        protected StreamAccelerometer CurrentStreamCommand { get; set; }

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

        public virtual Task<bool> ConfirmUserInteraction(int timeout)
        {
            throw new NotImplementedException();
        }

        public virtual async Task UpdateDeviceState()
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
            await _processor.AddCommand(new LED2Test());
            await _processor.AddCommand(new LED3Test());
        }

        public virtual Task WriteBitmap(string file)
        {
            throw new NotImplementedException();
        }

        public virtual Task WriteBitmap(byte[] data, int offset)
        {
            throw new NotImplementedException();
        }

        public virtual Task ClearDisplay()
        {
            throw new NotImplementedException();
        }

        public virtual Task DisplayIcon(int offset, int start = -1, int height = -1)
        {
            throw new NotImplementedException();
        }

        public virtual Task PaintDisplay(ushort offset, byte startCol, byte startRow, byte cols, byte rows, byte span)
        {
            throw new NotImplementedException();
        }

        public async Task StartAccelerometerStream(int rate = 0, int range = 0)
        {
            if (CurrentStreamCommand == null)
            {
                CurrentStreamCommand = new StreamAccelerometer(rate, range);
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
#if DEBUG_COMMS
                Console.WriteLine($"SYNC WARNING: Some data was lost from the device through the circular buffer was asking to read from {readFrom} to {readTo}, will now read from {newReadFrom}.");
#endif
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
				catch (Exception ex)
				{
                    // Worth a retry if it's a result of a possible temporary failure
                    if (ex is BlockSyncFailedException || ex is CommandFailedException)
					{
#if DEBUG_COMMS
						Console.WriteLine($"SYNC -- READ BLOCK {current} FAILED -- RESYNCING");
#endif
						// If read operation failure cause by device not writing block in time wait for 3 connection intervals and retry.
						Thread.Sleep((int)(ConnectionInterval * 3));
                        // Retry
                        try
                        {
                            await WriteCurrentBlock(current);
                            block = await SyncCurrentEpochBlock();
                        }
                        catch (BlockSyncFailedException)
                        {
                            throw;
                        }
                        catch (Exception ex2)
                        {
                            // The retry also failed
                            throw new BlockSyncFailedException("The retry failed after the original exception: " + ex.Message, current, null, ex2);
                        }
                    }
                    else
                    {
                        // An unanticipated problem
                        throw new BlockSyncFailedException("Unanticipated problem that was not retried.", current, null, ex);
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

            throw new BlockSyncFailedException("CRC check failed", block.BlockInfo.BlockNumber, block.Raw);
        }

        public async Task<BlockDetails> WriteCurrentBlock(UInt16 blockNo)
        {
            return await _processor.AddCommand(new WriteCurrentBlock(blockNo));
        }

        public async Task<BlockDetails> ReadBlockDetails()
        {
            return await _processor.AddCommand(new QueryBlockDetails());
        }

        public virtual Task WriteRealTime(DateTime time)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<string> DebugDump()
		{
			return await _processor.AddCommand(new DebugDump());
		}

        protected bool CheckCRC(byte[] data, ushort crc)
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

        protected EpochBlock[] CalculateTimestamps(EpochBlock[] blocks, UInt32? lastRtc, DateTimeOffset? lastSync, UInt32 currentRtc, DateTimeOffset currentTime)
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

        protected EpochBlock[] CalculateTimestampsForSet(EpochBlock[] blocks, UInt32 offsetRtc, DateTimeOffset offsetTime)
        {
            foreach (var block in blocks)
            {
                block.BlockInfo.Timestamp = CalculateTimestamp(block.BlockInfo.DeviceTimestamp, offsetRtc, offsetTime);
            }

            return blocks;
        }

        protected DateTimeOffset CalculateTimestamp(UInt32 timestamp, UInt32 currentRtc, DateTimeOffset currentTime)
        {
			return currentTime.AddSeconds((int) (timestamp - currentRtc));
        }

        protected async Task ReadBattery()
        {
            Battery = await _processor.AddCommand(new ReadBattery());
        }

        protected async Task ReadDeviceTime()
        {
            DeviceTime = await _processor.AddCommand(new ReadDeviceTime());
        }

        protected async Task ReadEraseData()
        {
            EraseData = await _processor.AddCommand(new QueryEraseData());
        }

        protected async Task ReadConnectionInterval()
        {
            _connectionInterval = await _processor.AddCommand(new ReadConnectionInterval());
        }

        protected async Task ReadCueingStatus()
        {
            var cueingConfig = await _processor.AddCommand(new QueryCueingConfig());

            _cueing = cueingConfig.Cueing;
            _cueingPeriod = cueingConfig.Period;
        }

        protected async Task ReadEpochPeriod()
        {
            _epochPeriod = await _processor.AddCommand(new ReadEpochPeriod());
        }

        protected async Task ReadGoalConfig()
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