using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reporting.Data;
using Reporting.Data.Repositories;

namespace Reporting.Services.UnitTests
{
    [TestClass]
    public class ArrivalServiceTests
    {
        private Mock<IArrivalRepository> _repositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [TestInitialize]
        public void Init()
        {
            _repositoryMock = new Mock<IArrivalRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(uow => uow.Arrivals).Returns(_repositoryMock.Object);
        }

        [TestMethod]
        public void Add()
        {
            _repositoryMock.Setup(x => x.Add(It.IsAny<Arrival>()));

            var arrival = new Arrival();
            var service = new ArrivalService(_unitOfWorkMock.Object);
            service.Add(arrival);

            _repositoryMock.Verify(x => x.Add(It.IsAny<Arrival>()), Times.Once);
        }

        [TestMethod]
        public void GetAll()
        {
            _repositoryMock.Setup(x => x.GetAll());

            var service = new ArrivalService(_unitOfWorkMock.Object);
            service.GetAll();

            _repositoryMock.Verify(x => x.GetAll(), Times.Once);
        }

        [TestMethod]
        public void RemoveAll()
        {
            _repositoryMock.Setup(x => x.RemoveAll());

            var service = new ArrivalService(_unitOfWorkMock.Object);
            service.RemoveAll();

            _repositoryMock.Verify(x => x.RemoveAll(), Times.Once);
        }
    }
}
