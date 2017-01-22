using Newtonsoft.Json;
using Reporting.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Reporting.Services
{
    public class DataService : IDataService
    {
        private const string WEB_SERVICE_TOKEN = "X-Fourth-Token";
        private const string ACCEPT_CLIENT_HEADER = "Fourth-Monitor";
        private  string ARRIVALS_URL = "api/clients/subscribe";

        public async Task<DataServiceResponse> SubscibeForData(string webServiceUrl, DateTime arrivalDay, string callback)
        {
            DataServiceResponse result = new DataServiceResponse();
            using (HttpClient client = new HttpClient())
            {
                string request = $"{webServiceUrl}{ARRIVALS_URL}?date={arrivalDay.ToString("yyyy-MM-dd")}&callback={callback}";
                client.BaseAddress = new Uri(webServiceUrl);
                client.DefaultRequestHeaders.Add("Accept-Client", ACCEPT_CLIENT_HEADER);

                HttpResponseMessage responseMessage = await client.GetAsync(request);
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result.IsSubsciptionSucceeded = true;
                    var body = await responseMessage.Content.ReadAsStringAsync();
                    result.Token = (WebServiceToken)JsonConvert.DeserializeObject(body, typeof(WebServiceToken));
                }
            }

            return result;
        }

        public bool ValidateData(HttpRequestBase request, WebServiceToken serviceToken)
        {
            if (request.Headers != null
                && request.Headers[WEB_SERVICE_TOKEN] != null
                && request.Headers[WEB_SERVICE_TOKEN] == serviceToken.Token)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Arrival> ParseData(HttpRequestBase request)
        {
            using (var stream = new StreamReader(request.InputStream))
            {
                string jsonData = stream.ReadToEnd();
                var arrivals = (List<Arrival>)JsonConvert.DeserializeObject(jsonData, typeof(List<Arrival>));

                return arrivals;
            }
        }
    }
}
