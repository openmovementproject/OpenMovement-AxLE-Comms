using System.Linq;
using OpenMovement.AxLE.Service.Models;

namespace OpenMovement.AxLE.Service.Interfaces
{
    public interface IReadOnlyRepository<T> where T : Model
    {
        T GetById(string id);
        IQueryable<T> GetAll();
    }
}
