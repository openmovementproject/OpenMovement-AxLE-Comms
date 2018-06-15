using System;
namespace OpenMovement.AxLE.Comms.Exceptions
{
    public class CommandFailedException : Exception
    {
        public string[] DataDump { get; }

        public CommandFailedException(string[] data) : base("Command failed, end of command was not found. Check DataDump field.")
        {
            DataDump = data;
        }

        public CommandFailedException(string[] data, Exception e) : base("Command failed, data returned was in an unexpected format. Check DataDump field and InnerException.", e)
        {
            DataDump = data;
        }

        public CommandFailedException(Exception e) : base("Command failed to execute, ensure the device is still in range. Check InnerExceiption for more details.", e) {}
    }
}
