using Reporting.Data;
using Reporting.Data.Repositories;
using System.Linq;

namespace Reporting.Services
{
    public class ArrivalService : IArrivalService
    {
        private IArrivalRepository _repository;
        public ArrivalService(IUnitOfWork unitOfWork)
        {
            _repository = unitOfWork.Arrivals;
        }
        public void Add(Arrival arrival)
        {
            _repository.Add(arrival);
        }
        public IQueryable<Arrival> GetAll()
        {
            return _repository.GetAll();
        }
        public void RemoveAll()
        {
            _repository.RemoveAll();
        }
    }
}
