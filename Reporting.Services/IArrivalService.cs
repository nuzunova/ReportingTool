using Reporting.Data;
using System.Linq;

namespace Reporting.Services
{
    public interface IArrivalService
    {
        void Add(Arrival arrival);
        IQueryable<Arrival> GetAll();
        void RemoveAll();
    }
}