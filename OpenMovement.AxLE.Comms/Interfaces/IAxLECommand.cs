using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Interfaces
{
    public interface IAxLECommand
    {
        IAxLEDevice Device { get; set; }
        Task Execute();
    }
}
