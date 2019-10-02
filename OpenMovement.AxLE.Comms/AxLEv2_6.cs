using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands.V1;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv2_6 : AxLEv2_4
    {
        public AxLEv2_6(IAxLEDevice device, string serial) : base(device, serial) { }

        public override async Task DisplayIcon(int offset, int start, int height)
        {
            await _processor.AddCommand(new DisplayIcon(offset, start, height));
        }

    }
}
