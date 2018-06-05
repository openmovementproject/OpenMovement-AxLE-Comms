using System;

namespace OpenMovement.AxLE.Service.Models
{
    public class EpochBlock : Model
    {
        public EpochBlockInfo BlockInfo { get; set; }

        public UInt16 BlockFormat { get; set; }

        public byte[] MetaData { get; set; } // 20

        public EpochSample[] Samples { get; set; } // 24

        public UInt16 CRC { get; set; }

        public byte[] Raw { get; set; } // 512

        public EpochBlock()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class EpochBlockInfo : Model
    {
        public UInt16 BlockNumber { get; set; }

        public UInt16 DataLength { get; set; }

        public UInt32 DeviceTimestamp { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public EpochBlockInfo()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class EpochSample : Model
    {
        public sbyte Battery { get; set; }

        public sbyte Temperatue { get; set; }

        public sbyte Acceleration { get; set; }

        public sbyte Steps { get; set; }

        public sbyte[] Epoch { get; set; } // 4

        public EpochSample()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
