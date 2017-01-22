using Reporting.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Reporting.Services
{
    public interface IDataService
    {
        IEnumerable<Arrival> ParseData(HttpRequestBase request);
        Task<DataServiceResponse> SubscibeForData(string webServiceUrl, DateTime arrivalDay, string callback);
        bool ValidateData(HttpRequestBase request, WebServiceToken serviceToken);
    }
}