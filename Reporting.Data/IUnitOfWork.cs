using Reporting.Data.Repositories;
using System.Threading.Tasks;

namespace Reporting.Data
{
    public interface IUnitOfWork
    {
        IArrivalRepository Arrivals { get; }

        Task CommitAsync();
    }
}