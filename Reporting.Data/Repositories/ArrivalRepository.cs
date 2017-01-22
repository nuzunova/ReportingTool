using Reporting.Data;
using System.Linq;

namespace Reporting.Data.Repositories
{
    public class ArrivalRepository : IArrivalRepository
    {
        private readonly ReportingDBEntities _db;
        public ArrivalRepository(ReportingDBEntities db)
        {
            _db = db;
        }

        public void Add(Arrival arrival)
        {
            _db.Arrivals.Add(arrival);
        }

        public IQueryable<Arrival> GetAll()
        {
            return _db.Arrivals;
        }

        public void RemoveAll()
        {
            _db.Arrivals.RemoveRange(_db.Arrivals);
        }
    }
}
