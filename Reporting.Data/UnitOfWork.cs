using Reporting.Data.Repositories;
using System.Threading.Tasks;

namespace Reporting.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ReportingDBEntities _db;
        private IArrivalRepository _arrivals;

        public IArrivalRepository Arrivals
        {
            get
            {
                if (_arrivals == null)
                {
                    _arrivals = new ArrivalRepository(_db);
                }
                return _arrivals;
            }
        }
        public UnitOfWork(ReportingDBEntities db)
        {
            _db = db;
        }

        public async Task CommitAsync()
        {
           await _db.SaveChangesAsync();
        }

    }
}
