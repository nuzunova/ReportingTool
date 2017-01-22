using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reporting.Controllers;
using Reporting.Data;
using Reporting.Services;
using Reporting.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Reporting.Web.UnitTests
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<IDataService> _dataServiceMock;
        private Mock<IArrivalService> _arrivalServiceMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [TestInitialize]
        public void Init()
        {
            _dataServiceMock = new Mock<IDataService>();
            _arrivalServiceMock = new Mock<IArrivalService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }
        [TestMethod]
        public async Task Index_ApplicationIsSubscribed_IndexViewIsReturnedWithArrivalsIfAny()
        {
            var arrivals = new List<Arrival>()
            {
               new Arrival() { EmployeeId = 1, When = DateTime.Now.AddMinutes(1)},
               new Arrival() { EmployeeId = 2, When = DateTime.Now.AddMinutes(2)},
            };

            _arrivalServiceMock
                .Setup(x => x.GetAll())
                .Returns(arrivals.AsQueryable());

            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);

            controller.SetFakeControllerContext("WebServiceToken", new WebServiceToken());
            var view = await controller.Index() as ViewResult;
            var model = view.Model as HomeViewModel;

            Assert.AreNotEqual("Error", view.ViewName);
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Arrivals);
            Assert.AreEqual(arrivals.Count, model.Arrivals.Count);
        }

        [TestMethod]
        public async Task Index_SubscriptionFails_ErrorViewIsReturned()
        {
            _dataServiceMock
                .Setup(x => x.SubscibeForData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Throws<Exception>();


            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext();
            
            var view = await controller.Index() as ViewResult;

            Assert.AreEqual("Error", view.ViewName);
        }

        [TestMethod]
        public async Task Index_SubscriptionSucceed_IndexViewIsReturnedWithArrivalsIfAny()
        {
            var arrivals = new List<Arrival>()
            {
               new Arrival() { EmployeeId = 1, When = DateTime.Now.AddMinutes(1)},
               new Arrival() { EmployeeId = 2, When = DateTime.Now.AddMinutes(2)},
            };

            _arrivalServiceMock
                .Setup(x => x.GetAll())
                .Returns(arrivals.AsQueryable());

            _dataServiceMock
                .Setup(x => x.SubscibeForData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(Task.FromResult( new DataServiceResponse()
                {
                    IsSubsciptionSucceeded = true,
                    Token = new WebServiceToken() { Token = "serviceToken" }
                }));


            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext();

            var view = await controller.Index() as ViewResult;
            var model = view.Model as HomeViewModel;

            Assert.AreNotEqual("Error", view.ViewName);
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Arrivals);
            Assert.AreEqual(arrivals.Count, model.Arrivals.Count);
        }

        [TestMethod]
        public async Task ReceiveDataFromService_NotValidToken_DataIsNotParsed()
        {
            _dataServiceMock.Setup(x => x.ValidateData(It.IsAny<HttpRequestBase>(), It.IsAny<WebServiceToken>())).Returns(false);
            _dataServiceMock.Setup(x => x.ParseData(It.IsAny<HttpRequestBase>()));

            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext();
            await controller.ReceiveDataFromService();

            _dataServiceMock.Verify(x => x.ParseData(It.IsAny<HttpRequestBase>()), Times.Never);
        }

        [TestMethod]
        public async Task ReceiveDataFromService_ValidToken_DataIsParsed()
        {
            _dataServiceMock.Setup(x => x.ValidateData(It.IsAny<HttpRequestBase>(), It.IsAny<WebServiceToken>())).Returns(true);
            _dataServiceMock.Setup(x => x.ParseData(It.IsAny<HttpRequestBase>()));

            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext();
            await controller.ReceiveDataFromService();

            _dataServiceMock.Verify(x => x.ParseData(It.IsAny<HttpRequestBase>()), Times.Once);
        }

        [TestMethod]
        public void CheckForUpdates_NewArrivals_ArrivalsParcialViewIsReturned()
        {
            var arrivals = new List<Arrival>()
            {
               new Arrival() { EmployeeId = 1, When = DateTime.Now.AddMinutes(1)},
               new Arrival() { EmployeeId = 2, When = DateTime.Now.AddMinutes(2)},
            };

            _arrivalServiceMock
                .Setup(x => x.GetAll())
                .Returns(arrivals.AsQueryable());

            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext("NeedRefresh", true);
            var view = controller.CheckForUpdates() as PartialViewResult;

            Assert.AreEqual("_Arrivals", view.ViewName);
        }

        [TestMethod]
        public void CheckForUpdates_ThereAreNoNewArrivals_EmptyResultIsReturned()
        {
            var arrivals = new List<Arrival>()
            {
               new Arrival() { EmployeeId = 1, When = DateTime.Now.AddMinutes(1)},
               new Arrival() { EmployeeId = 2, When = DateTime.Now.AddMinutes(2)},
            };

            _arrivalServiceMock
                .Setup(x => x.GetAll())
                .Returns(arrivals.AsQueryable());

            var controller = new HomeController(_dataServiceMock.Object, _arrivalServiceMock.Object, _unitOfWorkMock.Object);
            controller.SetFakeControllerContext("NeedRefresh", false);
            var view = controller.CheckForUpdates() as EmptyResult;

            Assert.IsNotNull(view);
        }
    }

    internal static class FakeControllerContext
    {
        public static ControllerContext FakeHttpContext(string key, object value)
        {
            var httpRequest = new Mock<HttpRequestBase>();
            var httpContext = new Mock<HttpContextBase>();
            var controllerContext = new Mock<ControllerContext>();
            var application = new Mock<HttpApplicationStateBase>();

            if (!string.IsNullOrEmpty(key))
            {
                application.SetupGet(ctx => ctx[key]).Returns(value);
            }


            httpRequest.SetupGet(r => r.Url).Returns(new Uri("http://myWeb"));
            controllerContext.SetupGet(ctx => ctx.HttpContext).Returns(httpContext.Object);
            httpContext.SetupGet(c => c.Request).Returns(httpRequest.Object);
            httpContext.SetupGet(ctx => ctx.Application).Returns(application.Object);
            return controllerContext.Object;
        }

        public static void SetFakeControllerContext(this Controller controller, string key = "", object value = null)
        {
            var context = FakeHttpContext(key, value);
            controller.ControllerContext = context;
            controller.Url = new UrlHelper(
             new RequestContext(
                 controller.HttpContext, new RouteData()
             ),
             new RouteCollection()
         );
        }
    }
}
