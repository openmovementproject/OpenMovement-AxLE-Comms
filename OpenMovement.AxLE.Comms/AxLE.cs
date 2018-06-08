using System;
using OpenMovement.AxLE.Comms.Commands;
using OpenMovement.AxLE.Comms.Values;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenMovement.AxLE.Comms.Interfaces;
using OpenMovement.AxLE.Service.Models;
using OpenMovement.AxLE.Comms.Exceptions;
using System.Linq;

namespace OpenMovement.AxLE.Comms
{
    public class AxLE : IDisposable
    {
        private readonly IAxLEDevice _device;
        private readonly IAxLEProcessor _processor;

        /// <summary>
        /// Gets the AxLE's Bluetooth device identifier (specific to install).
        /// </summary>
        /// <value>The device identifier.</value>
        public string DeviceId { get; }
        /// <summary>
        /// Gets the AxLE's serial number Format=[XXXXXXXXXXXX].
        /// </summary>
        /// <value>The serial number.</value>
        public string SerialNumber { get; }
        /// <summary>
        /// Gets the battery percentage.
        /// </summary>
        /// <value>The battery percentage.</value>
        public int Battery { get; private set; }
        /// <summary>
        /// AxLE local device time in seconds.
        /// </summary>
        /// <value>The device time.</value>
        public UInt32 DeviceTime { get; private set; }
        /// <summary>
        /// AxLE devices' historic erase data.
        /// </summary>
        /// <value>The erase data.</value>
        public EraseData EraseData { get; private set; }

        /// <summary>
        /// BLE device connection interval. Fires off command to device.
        /// </summary>
        private UInt16 _connectionInterval;
        public UInt16 ConnectionInterval
        {
            get => _connectionInterval;
            set
            {
                _connectionInterval = value;
                _processor.AddCommand(new WriteConnectionInterval(ConnectionInterval));
            }
        }

        /// <summary>
        /// Turn cueuing on or off for 60 seconds. Fires off command to device.
        /// </summary>
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

        /// <summary>
        /// Get or Set cueing period in seconds. Fires off command to device.
        /// </summary>
        private UInt16 _cueingPeriod;
        public UInt16 CueingPeriod
        {
            get => _cueingPeriod;
            set
            {
                _cueingPeriod = value;
                _processor.AddCommand(new WriteCueingPeriod(CueingPeriod));
            }
        }

        /// <summary>
        /// Get or Set Epoch period in seconds. Fires off command to device.
        /// </summary>
        private UInt16 _epochPeriod;
        public UInt16 EpochPeriod
        {
            get => _epochPeriod;
            set
            {
                _epochPeriod = value;
                _processor.AddCommand(new WriteEpochPeriod(EpochPeriod));
            }
        }

        /// <summary>
        /// Get or Set the goal period offset in seconds. Fires off command to device.
        /// </summary>
        private UInt16 _goalPeriodOffset;
        public UInt16 GoalPeriodOffset
        {
            get => _goalPeriodOffset;
            set
            {
                _goalPeriodOffset = value;
                _processor.AddCommand(new WriteGoalPeriodOffset(GoalPeriodOffset));
            }
        }

        /// <summary>
        /// Get or Set the goal period in seconds. Fires off command to device.
        /// </summary>
        private UInt16 _goalPeriod;
        public UInt16 GoalPeriod
        {
            get => _goalPeriod;
            set
            {
                _goalPeriod = value;
                _processor.AddCommand(new WriteGoalPeriod(GoalPeriod));
            }
        }

        /// <summary>
        /// Get or Set the step goal threshold (vibrates when exceeded in 24hr period). Fires off command to device.
        /// </summary>
        private UInt16 _goalThreshold;
        public UInt16 GoalThreshold
        {
            get => _goalThreshold;
            set
            {
                _goalThreshold = value;
                _processor.AddCommand(new WriteGoalThreshold(GoalThreshold));
            }
        }

        private StreamAccelerometer CurrentStreamCommand { get; set; }

        /// <summary>
        /// Subscribe to this callback when running accelerometer stream, called when new stream block is recieved.
        /// </summary>
        public event EventHandler<AccBlock> AccelerometerStream;

        public AxLE(IAxLEDevice device, string serial)
        {
            _device = device;
            _processor = new AxLEProcessor(device);

            DeviceId = _device.DeviceId;
            SerialNumber = serial;

            _processor.StartProcessor();
        }

        /// <summary>
        /// Authenticate with AxLE device using specified password (default password is last 6 digits of serial number).
        /// </summary>
        /// <returns>Success or failure of auth.</returns>
        /// <param name="password">Password.</param>
        public async Task<bool> Authenticate(string password)
        {
            return await _processor.AddCommand(new Unlock(password));
        }

        /// <summary>
        /// Queries various properties of the device and updates AxLE object. Not run on construction for performance reasons.
        /// </summary>
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

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="password">Password.</param>
        public async Task SetPassword(string password)
        {
            await _processor.AddCommand(new SetPassword(password));
        }

        /// <summary>
        /// Resets the password to last six digits of MAC address (THIS ERASES ALL DEVICE DATA).
        /// </summary>
        public async Task ResetPassword()
        {
            await _processor.AddCommand(new ResetPasswordAndErase(SerialNumber.Substring(SerialNumber.Length - 6)));
        }

        /// <summary>
        /// Vibrates the device.
        /// </summary>
        public async Task VibrateDevice()
        {
            await _processor.AddCommand(new MotorPulse());
        }

        /// <summary>
        /// Starts streaming accelerometer data. Subscribe to <see cref="AccelerometerStream"/> event to get data.
        /// </summary>
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

        /// <summary>
        /// Stops streaming accelerometer data.
        /// </summary>
        public async Task StopAccelerometerStream()
        {
            if (CurrentStreamCommand != null)
            {
                await CurrentStreamCommand.StopStream();
                CurrentStreamCommand = null;
            }
        }

        /// <summary>
        /// Syncs the Epoch data from the device, operation may take some time. This overload reads to the active block.
        /// </summary>
        /// <exception cref="InvalidBlockRangeException">Thrown when a the requested range exceeds the device storage. You must use the other overload to read.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="lastRtc"/> does not match the band data.</exception>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check twice.</exception>
        /// <returns>The synced Epoch data.</returns>
        /// <param name="lastBlock">Last block read from device, if first run read ActiveBlock from <see cref="ReadBlockDetails"/>.</param>
        /// <param name="lastRtc">Device clock at last sync.</param>
        /// <param name="lastSync">Global time at last sync.</param>
        public async Task<EpochBlock[]> SyncEpochData(UInt16 lastBlock, UInt32? lastRtc = null, DateTimeOffset? lastSync = null)
        {
            var blockDetails = await _processor.AddCommand(new QueryBlockDetails());
            return await SyncEpochData(lastBlock, blockDetails.ActiveBlock, lastRtc, lastSync);
        }

        /// <summary>
        /// Syncs the Epoch data from the device, operation may take some time.
        /// </summary>
        /// <exception cref="InvalidBlockRangeException">Thrown when a the requested range exceeds the device storage.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="lastRtc"/> does not match the band data.</exception>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check twice.</exception>
        /// <returns>The synced Epoch data.</returns>
        /// <param name="readFrom">Block to read from, if first run read ActiveBlock from <see cref="ReadBlockDetails"/>.</param>
        /// <param name="readTo">Block number to read to, usually ActiveBlock.</param>
        /// <param name="startRtc">Device clock at beginning of block read range.</param>
        /// <param name="startTime">Global time at beginning of block read range.</param>
        public async Task<EpochBlock[]> SyncEpochData(UInt16 readFrom, UInt16 readTo, UInt32? startRtc = null, DateTimeOffset? startTime = null)
        {
            if (((UInt16)(readTo - readFrom)) > AxLEConfig.BlockCount)
                throw new InvalidBlockRangeException(readFrom, readTo);

            var blocks = new List<EpochBlock>();

            await _processor.AddCommand(new HighSpeedMode());

            var blockDetails = await WriteCurrentBlock(readFrom);

            var last = readFrom;
            while (last != readTo)
            {
                EpochBlock block = null;
                try
                {
                    block = await SyncCurrentEpochBlock();
                }
                catch (BlockSyncFailedException)
                {
                    var blockFailed = (ushort)(last + 1);
#if DEBUG_COMMS
                    Console.WriteLine($"SYNC -- READ BLOCK {blockFailed} FAILED -- RESYNCING");
#endif              
                    await WriteCurrentBlock(blockFailed);
                    block = await SyncCurrentEpochBlock();
                }
#if DEBUG_COMMS
                Console.WriteLine($"SYNC -- Read Block: {block.BlockInfo.BlockNumber}");
#endif
                last = block.BlockInfo.BlockNumber;

                blocks.Add(block);
            }
#if DEBUG_COMMS
            Console.WriteLine($"SYNC COMPLETE -- Blocks Read: {blocks.Count}");
#endif

            await _processor.AddCommand(new LowPowerMode());

            await ReadDeviceTime();

            return CalcualateTimestamps(blocks.ToArray(), startRtc, startTime, DeviceTime, DateTime.UtcNow);
        }

        /// <summary>
        /// Syncs the Epoch block referenced by the DownloadBlock pointer from <see cref="ReadBlockDetails"/>.
        /// </summary>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check.</exception>
        /// <returns>The current Epoch block.</returns>
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

        /// <summary>
        /// Writes the current block to be read.
        /// </summary>
        /// <returns>The block details.</returns>
        /// <param name="blockNo">Block no.</param>
        public async Task<BlockDetails> WriteCurrentBlock(UInt16 blockNo)
        {
            return await _processor.AddCommand(new WriteCurrentBlock(blockNo));
        }

        /// <summary>
        /// Reads the block details from the device, run this operation on setup get starting block.
        /// </summary>
        /// <returns>The block details.</returns>
        public async Task<BlockDetails> ReadBlockDetails()
        {
            return await _processor.AddCommand(new QueryBlockDetails());
        }

        /// <summary>
        /// Flash Blue LED 3 on device.
        /// </summary>
        public async Task LEDFlash()
        {
            await _processor.AddCommand(new LED3Test());
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

        private EpochBlock[] CalcualateTimestamps(EpochBlock[] blocks, UInt32? lastRtc, DateTimeOffset? lastSync, UInt32 currentRtc, DateTimeOffset currentTime)
        {
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

            if (sets.Count < 1)
                return new EpochBlock[0];

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
            var offsetTime = lastSync.Value.AddSeconds(offsetRtc - startSet.First().BlockInfo.DeviceTimestamp);

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
            return currentTime.AddSeconds(timestamp - currentRtc);
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