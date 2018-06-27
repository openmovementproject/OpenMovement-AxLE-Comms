using System;
namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class BlockSyncFailedException : Exception
    {
        public UInt16 BlockNumber { get; }
        public byte[] DataDump { get; }

        public BlockSyncFailedException(string detail, UInt16 blockNumber, byte[] data, Exception innerException = null) : base("Block failed to sync. " + (detail ?? ""), innerException)
        {
            BlockNumber = blockNumber;
            DataDump = data;
        }
    }
}
