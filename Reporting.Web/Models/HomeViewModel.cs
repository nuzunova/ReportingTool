using Reporting.Data;

namespace Reporting.Web.Models
{
    public class HomeViewModel
    {
        public PagedList.IPagedList<Arrival> Arrivals { get; set; }
    }
}