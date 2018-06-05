using System;

namespace OpenMovement.AxLE.Comms.Values
{
    public struct BlockDetails
    {
        public UInt32 Time { get; set; }

        public UInt16 ActiveBlock { get; set; }

        public UInt16 ActiveSamples { get; set; }

        public UInt32 ActiveEpoch { get; set; }

        public UInt16 BlockCount { get; set; }

        public UInt16 DownloadBlock { get; set; }
    }
}
