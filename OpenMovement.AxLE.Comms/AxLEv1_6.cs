using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands.V2;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv1_6 : AxLEv1_5
    {
        public AxLEv1_6(IAxLEDevice device, string serial) : base (device, serial) {}
        
		public override async Task<string> DebugDump()
        {
			var oldDebug = await base.DebugDump();
			var newDebug = await _processor.AddCommand(new DebugDump());
			return oldDebug + newDebug;
        }
    }
}
