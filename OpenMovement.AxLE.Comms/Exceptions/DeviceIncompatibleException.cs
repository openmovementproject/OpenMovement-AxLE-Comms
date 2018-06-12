using System;
namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class DeviceIncompatibleException : Exception
    {
        public DeviceIncompatibleException(string message) : base(message) {}
    }
}
