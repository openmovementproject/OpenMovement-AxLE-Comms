using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands.V1;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv2_3 : AxLEv1_7
    {
        public AxLEv2_3(IAxLEDevice device, string serial) : base(device, serial) { }

        public override async Task<bool> ConfirmUserInteraction(int timeout)
        {
            return await _processor.AddCommand(new ConfirmUserInteraction(timeout));
        }
    }
}
