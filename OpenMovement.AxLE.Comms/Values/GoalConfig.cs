using System;

namespace OpenMovement.AxLE.Comms.Values
{
    public struct GoalConfig
    {
        public UInt16 GoalPeriod { get; set; }

        public UInt16 GoalPeriodOffset { get; set; }

        public UInt16 GoalThreshold { get; set; }
    }
}
