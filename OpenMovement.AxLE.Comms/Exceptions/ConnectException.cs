using System;

namespace OpenMovement.AxLE.Comms.Exceptions
{
    class ConnectException : Exception
    {
        public ConnectException(Exception e) : base("The device was found but the connect failed. Most likely the device is not nearby the device or just retry connection.", e) { }
    }
}
