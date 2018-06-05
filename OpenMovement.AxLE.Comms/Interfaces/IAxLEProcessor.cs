using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands;

namespace OpenMovement.AxLE.Comms.Interfaces
{
    public interface IAxLEProcessor : IDisposable
    {
        void StartProcessor();
        void StopProcessor();

        /// <summary>
        /// Adds command to be run on AxLE device. Will not neccesarily be run immediately.
        /// </summary>
        /// <returns>The result of the command.</returns>
        /// <param name="command">Command to be run.</param>
        /// <typeparam name="T">Command return <typeparamref name="T"/>.</typeparam>
        Task<T> AddCommand<T>(AxLECommand<T> command);
        /// <summary>
        /// Adds stream command to be run on AxLE device. Will not neccesarily be run immediately.
        /// </summary>
        /// <returns>When stream command has been explicitly stopped.</returns>
        /// <param name="command">Stream command to be run.</param>
        /// <typeparam name="T">Update return type of command <typeparamref name="T"/>.</typeparam>
        Task AddCommand<T>(AxLECommandStream<T> command);
    }
}
