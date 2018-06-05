using System;
using System.Threading.Tasks;

namespace OpenMovement.AxLE.Comms.Bluetooth.Interfaces
{
    public interface IDevice : IDisposable
    {
        string Id { get; }
        string MacAddress { get; }
        string Name { get; }
        int Rssi { get; }

        Task<IService> GetService(Guid service);
    }

    public interface IService : IDisposable
    {
        Guid Id { get; }

        Task<ICharacteristic> GetCharacteristic(Guid charac);
    }

    public interface ICharacteristic : IDisposable
    {
        Guid Id { get; }

        event EventHandler<byte[]> ValueUpdated;

        Task RegisterForNotifications();
        Task RegisterForIndications();

        Task UnregisterForUpdates();

        Task Write(byte[] data);
        Task<byte[]> Read();
    }
}
