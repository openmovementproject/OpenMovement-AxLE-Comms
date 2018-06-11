using System;

namespace OpenMovement.AxLE.Comms.Values
{
    public struct EraseData
    {
        public double BatteryCycles { get; set; }

        public UInt32 ResetCycles { get; set; }

        public UInt32 EraseCycles { get; set; }
    }
}
