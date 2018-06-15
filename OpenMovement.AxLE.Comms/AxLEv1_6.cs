using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEv1_6 : AxLEv1_5
    {
        public AxLEv1_6(IAxLEDevice device, string serial) : base (device, serial) {}
    }
}
