using System;

namespace OpenMovement.AxLE.Comms.Commands
{
    public abstract class AxLECommandNoResponse : AxLECommand<int>
    {
        protected override bool LookForEnd()
        {
            // TODO: Search for !
            return true;
        }

        protected override int ProcessResult()
        {
            return 0;
        }
    }
}
