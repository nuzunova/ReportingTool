using Reporting.Data;
using System.Linq;

namespace Reporting.Data.Repositories
{
    public interface IArrivalRepository
    {
        void Add(Arrival arrival);
        void RemoveAll();
        IQueryable<Arrival> GetAll();
    }
}