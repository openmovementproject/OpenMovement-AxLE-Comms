using System;
namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class CommandFailedException : Exception
    {
        public string Command { get; set; }

        public string[] DataDump { get; set; }

        public CommandFailedException(string command)
        {
            Command = command;
        }

        public CommandFailedException(string[] data)
        {
            DataDump = data;
        }
    }
}
