using System;

namespace OpenMovement.AxLE.Comms.Values
{
    public struct GoalConfig
    {
        public UInt32 GoalPeriod { get; set; }

        public UInt32 GoalPeriodOffset { get; set; }

        public UInt32 GoalThreshold { get; set; }
    }
}
