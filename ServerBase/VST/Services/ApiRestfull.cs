using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ApiRestfull
    {
        public string Protocol { get; set; } = "http";
        public string HostName { get; set; }
        public string Action { get; set; }

        public string CreateUrl() => $"{Protocol}://{HostName}/api/{Action}";


        public Document ResponseContext { get; protected set; }
        protected virtual void RaiseResponseReceived(string response)
        {
        }

        protected virtual void RaiseCompleted(Document context)
        {
        }

        protected virtual void RaiseRequestError(string message)
        {

        }

        public async Task<Document> Request(Document param) => await Request(param, null);
        public async Task<Document> Request(Document param, Action<Document> callback)
        {
            try
            {
                var content = new StringContent(param.ToString(), Encoding.ASCII, "application/json");
                var url = CreateUrl();
                var client = new HttpClient();

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    RaiseResponseReceived(responseData);

                    ResponseContext = Document.Parse(responseData);
                    RaiseCompleted(ResponseContext);

                    callback?.Invoke(ResponseContext);

                    return ResponseContext;
                }
            }
            catch (Exception ex)
            {
                RaiseRequestError(ex.Message);
            }

            return null;
        }
    }
}
