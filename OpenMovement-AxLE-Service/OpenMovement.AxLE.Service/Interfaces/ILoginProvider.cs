using System;
using System.Threading.Tasks;
using OpenMovement.AxLE.Service.Models;

namespace OpenMovement.AxLE.Service.Interfaces
{
    /// <summary>
    /// OpenMovement Login Provider. Implementations of this interface should handle all neccesary UI prompts and return an authenticated Repository for storage of OpenMovement AxLE data.
    /// </summary>
    public interface ILoginProvider<T> where T : Model
    {
        /// <summary>
        /// This is the main function for logging a user into the relevant provider. All UI prompts should be contained within this function.
        /// </summary>
        /// <returns>An authenticated data store for Band data.</returns>
        Task<IReadWriteRepository<T>> Login();
        /// <summary>
        /// If required a callback with authentication tokens can be used here.
        /// </summary>
        /// <param name="uri">URI with tokens for authentication.</param>
        void LoginCallback(Uri uri);
    }
}
