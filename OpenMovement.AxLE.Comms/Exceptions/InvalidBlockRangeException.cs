using System;

namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class InvalidBlockRangeException : Exception
    {
        public UInt16 MaxBlocks { get; }
        public UInt16 FromBlock { get; }
        public UInt16 ToBlock { get; }

        public InvalidBlockRangeException(UInt16 fromBlock, UInt16 toBlock) : base($"You have exceeded the max block range ({AxLEConfig.BlockCount} blocks), " +
                                                                                   $"with the request for {(UInt16) (toBlock - fromBlock)} blocks.")
        {
            MaxBlocks = AxLEConfig.BlockCount;
            FromBlock = fromBlock;
            ToBlock = toBlock;
        }
    }
}
