using System;
namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class BlockSyncFailedException : Exception
    {
        public UInt16 BlockNumber { get; }
        public byte[] DataDump { get; }

        public BlockSyncFailedException(UInt16 blockNumber, byte[] data) : base("Block failed to sync, CRC check failed.")
        {
            BlockNumber = blockNumber;
            DataDump = data;
        }
    }
}
