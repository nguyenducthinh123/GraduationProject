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
    public class ElectricalReader : ApiRestfull
    {
        public ElectricalReader()
        {
            HostName = "113.160.87.222:5000";
        }

        string _deviceId;
        protected override void RaiseRequestError(string message)
        {
            Console.Write(Screen.Now($"{_deviceId}"));
            Screen.Error($"{HostName} fail");
            base.RaiseRequestError(message);
        }

        public async Task<DocumentList> GetHistory(string deviceId, DateTime? startTime, DateTime? endTime = null, string field = null)
        {
            const string timeFormat = "yyyy-MM-dd HH:mm:ss";
            var param = new Document {
                { "id", _deviceId = deviceId },
                { "fromDate", startTime.Value.ToString(timeFormat) },
                { "toDate", (endTime ?? startTime.Value.AddMinutes(1)).ToString(timeFormat) },
            };

            if (field != null)
                param.Add("field", field);

            Action = "data";
            var doc = await Request(param);
            if (doc != null)
            {
                var res = doc.GetDocumentList("result");
                if (res != null && res.Count > 0)
                {
                    return res[0].GetDocumentList("data");
                }
            }

            return new DocumentList();
        }

        public async Task<Document> OnOff(string id, string action)
        {
            Action = "control";
            var param = new Document {
                { "id", id },
                { "action", action },
                { "relay", 0 }
            };

            return await Request(param, r => Screen.Success(r.ToString()));
        }
    }
}
