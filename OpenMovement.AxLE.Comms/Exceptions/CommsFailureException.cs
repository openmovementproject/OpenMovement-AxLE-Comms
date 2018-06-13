using System;

namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class CommsFailureException : Exception
    {
        public CommsFailureException(Exception e) : base("Failed to open Comms with the device. Check the inner exception for more details.", e) {}
    }
}
