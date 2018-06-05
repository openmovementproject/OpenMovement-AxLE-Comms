using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Interfaces
{
    public interface IAxLEDevice : IDisposable
    {
        string DeviceId { get; }
        Task TxUart(string data);
        event EventHandler<string> RxUart;
    }
}
