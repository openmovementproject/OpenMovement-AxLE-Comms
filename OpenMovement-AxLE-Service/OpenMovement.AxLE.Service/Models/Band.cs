using System;
using System.Collections.Generic;

namespace OpenMovement.AxLE.Service.Models
{
    public class Band : Model
    {
        public string SerialNumber { get; set; }

        public string Password { get; set; }

        public bool BackgroundSync { get; set; }

        public UInt32 LastDeviceRTC { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        public DateTimeOffset LastSync { get; set; }

        public UInt16 LastBlockRead { get; set; }

        public ICollection<EpochBlock> Data { get; set; }

        public Band()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
