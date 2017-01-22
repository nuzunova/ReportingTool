using log4net;
using PagedList;
using Reporting.Data;
using Reporting.Services;
using Reporting.Web.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting.Controllers
{
    public class HomeController : Controller
    {
        private const string DEFAULT_SORTING = "when_desc";
        private const int DEFAULT_PAGE = 1;
        private const string LOGGER_FILE = "HomeComtroller";

        private IDataService _dataService;
        private IArrivalService _arrivalService;
        private IUnitOfWork _unitOfWork;
      
        private int _pageSize = 10;

        public HomeController(IDataService dataService, IArrivalService arrivalService, IUnitOfWork unitOfWork)
        {
            _dataService = dataService;
            _arrivalService = arrivalService;
            _unitOfWork = unitOfWork;
         
            if (ConfigurationManager.AppSettings["PageSize"] != null)
            {
                _pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
            }
        }

        public async Task<ActionResult> Index()
        {
            if (IsApplicationSubscribedForData())
            {
                return GetIdexViewWithArrivals();
            }

            ////Uncomment if you want to delete data and start pushing data from beginning:
            //_arrivalService.RemoveAll();
            //await _unitOfWork.CommitAsync(CancellationToken.None);

            string webServiceUrl = ConfigurationManager.AppSettings["ArrivalWebServiceURL"];
            string callback = Url.Action("ReceiveDataFromService", "Home", null, Request.Url.Scheme);
            DateTime arrivalDay = new DateTime(2016, 3, 10);

            bool success = false;
            try
            {
                var result = await _dataService.SubscibeForData(webServiceUrl, arrivalDay, callback);
                if (result != null && result.IsSubsciptionSucceeded)
                {
                    StoreWebServiceToken(result.Token);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(LOGGER_FILE).Error("Subscription for receiving arrival data failed.", ex);
            }

            if (!success)
            {
                return View("Error");
            }

            return GetIdexViewWithArrivals();
        }

        public async Task<HttpResponseMessage> ReceiveDataFromService()
        {
            var webServiceToken = GetWebServiceToken();
            bool valid = _dataService.ValidateData(Request, webServiceToken);
         
            if (valid)
            {
                try
                {
                    var arrivals = _dataService.ParseData(Request);
                    foreach (var arrival in arrivals)
                    {
                        _arrivalService.Add(arrival);
                    }
                    await _unitOfWork.CommitAsync();
                    HttpContext.Application["NeedRefresh"] = true;
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(LOGGER_FILE).Error("Error on receiving data from service", ex);
                }
            }

            return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK };
        }

        public ActionResult LoadArrivals(string sortOrder = DEFAULT_SORTING, int page = 1)
        {
            var arrivals = GetArrivals(sortOrder, page, _pageSize);

            return PartialView("_Arrivals", arrivals);
        }

        public ActionResult CheckForUpdates()
        {
            if (HttpContext.Application["NeedRefresh"] != null
                && (bool)HttpContext.Application["NeedRefresh"])
            {
                HttpContext.Application["NeedRefresh"] = false;
                return LoadArrivals();
            }
            return new EmptyResult();
        }

     
        private bool IsApplicationSubscribedForData()
        {
            var webServiceToken = GetWebServiceToken();

            return webServiceToken != null;
        }
        private void StoreWebServiceToken(WebServiceToken token)
        {
            HttpContext.Application["WebServiceToken"] = token;
        }
        private WebServiceToken GetWebServiceToken()
        {
            if (HttpContext.Application["WebServiceToken"] != null)
            {
                return (WebServiceToken)HttpContext.Application["WebServiceToken"];
            }

            return null;
        }
        private ActionResult GetIdexViewWithArrivals()
        {
            var arrivals = GetArrivals(DEFAULT_SORTING, DEFAULT_PAGE, _pageSize);

            return View(new HomeViewModel()
            {
                Arrivals = arrivals
            });
        }
        private IPagedList<Arrival> GetArrivals(string sortOrder, int page, int pageSize)
        {
            ViewBag.EmployeeIdSortParam = sortOrder == "employeeId" ? "employeeId_desc" : "employeeId";
            ViewBag.WhenSortParam = sortOrder == "when" ? "when_desc" : "when";
            ViewBag.CurrentSort = sortOrder;

            var arrivals = _arrivalService.GetAll();

            switch (sortOrder)
            {
                case "employeeId":
                    arrivals = arrivals.OrderBy(x => x.EmployeeId);
                    break;
                case "when":
                    arrivals = arrivals.OrderBy(x => x.When);
                    break;
                case "employeeId_desc":
                    arrivals = arrivals.OrderByDescending(x => x.EmployeeId);
                    break;
                case "when_desc":
                    arrivals = arrivals.OrderByDescending(x => x.When);
                    break;
                default:
                    break;
            }

            try
            {
                return arrivals.ToPagedList(page, pageSize);
            }
            catch(Exception ex)
            {
                LogManager.GetLogger(LOGGER_FILE).Error("Error on getting arrivals from database", ex);
                return null;
            }
           
        }
    } 
}