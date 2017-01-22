using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reporting.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Reporting.Services.UnitTests
{
    [TestClass]
    public class DataServiceTests
    {
        private DateTime arrivalDay = new DateTime(2016, 3, 10);

        [TestMethod]
        [ExpectedException(typeof(System.UriFormatException))]
        public async Task SubscibeForData_WrongServiceUrl_ExceptionIsThrow()
        {
            var service = new DataService();
            await service.SubscibeForData("wrongUrl", arrivalDay, "http://myWebApp");
        }

        [TestMethod]
        public async Task SubscibeForData_WrongCallbackUrl_NotSuccessfulSubscribe()
        {
            var service = new DataService();
            var result = await service.SubscibeForData("http://localhost:51396/", arrivalDay, "wrongCallback");

            Assert.IsFalse(result.IsSubsciptionSucceeded);
            Assert.IsNull(result.Token);
        }

        [TestMethod]
        public async Task SubscibeForData_CorrectUrls_SuccessfulSubscribe()
        {
            var service = new DataService();
            var result = await service.SubscibeForData("http://localhost:51396/", arrivalDay, "http://myWebApp");

            Assert.IsTrue(result.IsSubsciptionSucceeded);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public void ValidateData_NoHeaders_ReturnFalse()
        {
            var token = new WebServiceToken()
            {
                Token = "token",
                Expires = DateTime.Now.AddDays(5)
            };

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns((System.Collections.Specialized.NameValueCollection)null);

            var service = new DataService();
            var valid = service.ValidateData(request.Object, token);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void ValidateData_NoHeaderToken_ReturnFalse()
        {
            var token = new WebServiceToken()
            {
                Token = "token",
                Expires = DateTime.Now.AddDays(5)
            };

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(new System.Collections.Specialized.NameValueCollection());

            var service = new DataService();
            var valid = service.ValidateData(request.Object, token);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void ValidateData_NotMatchedToken_ReturnFalse()
        {
            var token = new WebServiceToken()
            {
                Token = "token",
                Expires = DateTime.Now.AddDays(5)
            };

            var request = new Mock<HttpRequestBase>();

            var headers = new System.Collections.Specialized.NameValueCollection() { };
            headers.Add("X-Fourth-Token", "anotherToken");

            request.SetupGet(x => x.Headers).Returns(headers);

            var service = new DataService();
            var valid = service.ValidateData(request.Object, token);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void ValidateData_CorrectToken_ReturnTrue()
        {
            var token = new WebServiceToken()
            {
                Token = "token",
                Expires = DateTime.Now.AddDays(5)
            };

            var request = new Mock<HttpRequestBase>();

            var headers = new System.Collections.Specialized.NameValueCollection() { };
            headers.Add("X-Fourth-Token", "token");

            request.SetupGet(x => x.Headers).Returns(headers);

            var service = new DataService();
            var valid = service.ValidateData(request.Object, token);

            Assert.IsTrue(valid);
        }

        [TestMethod]
        [ExpectedException(typeof(Newtonsoft.Json.JsonReaderException))]
        public void ParseData_ArrivalsNotCorrectJson_ExceptionIsThrows()
        {
            var arrivalJson = "not corrrect json for arrivals";
            using (var stram = new MemoryStream(Encoding.UTF8.GetBytes(arrivalJson)))
            {
                var request = new Mock<HttpRequestBase>();
                request.SetupGet(x => x.InputStream).Returns(stram);

                var service = new DataService();
                service.ParseData(request.Object);
            }
 
        }


        [TestMethod]
        public void ParseData_ArrivalsCorrectJson_ParseIsSuccessful()
        {
            var arrivals = new List<Arrival>()
            {
                new Arrival() { EmployeeId = 1, When = DateTime.Now.AddHours(-1)},
                 new Arrival() { EmployeeId = 2, When = DateTime.Now.AddHours(-2)}
            };

            var arrivalJson = Newtonsoft.Json.JsonConvert.SerializeObject(arrivals);

            IEnumerable<Arrival> returnedArrivals = null;
            using (var stram = new MemoryStream(Encoding.UTF8.GetBytes(arrivalJson)))
            {
                var request = new Mock<HttpRequestBase>();
                request.SetupGet(x => x.InputStream).Returns(stram);

                var service = new DataService();
                returnedArrivals = service.ParseData(request.Object);
            }

            Assert.IsNotNull(returnedArrivals);
            Assert.IsTrue(returnedArrivals.Count() == 2);
        }
    }
}
