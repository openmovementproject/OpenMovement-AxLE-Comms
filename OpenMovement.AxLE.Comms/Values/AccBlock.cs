using System;

namespace OpenMovement.AxLE.Comms.Values
{
    public struct AccBlock
    {
        public UInt32 Timestamp { get; set; }

        public UInt16 Battery { get; set; }

        public UInt16 Temperature { get; set; }

        public Int16[][] Samples { get; set; }
    }
}
