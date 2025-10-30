using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace History.Controller
{
    class LogController : Vst.Server.Controller
    {
        protected Document Now()
        {
            var time = Payload.GetDateTime("t");
            if (time != null)
            {
                var id = RequestContext.ClientId;
                var device = Server.GetDevice(id);
                device.Logs.Add(Payload);
                device.GetSummary(doc => {
                    doc.Add(time);
                });

                Server.MainDB.Devices.Update(device);

                Server.GetDayHistory(id, time, (k, doc) => {
                    var items = doc.Items ?? new DocumentList();
                    Payload.Push("t", $"{time:HH:mm:ss}");

                    items.Add(Payload);
                    doc.Items = items;

                    Server.MainDB.EveryDay.Update(doc);
                });

                Screen.Warning(Screen.Now(id));
            }

            return null;
        }

        // history/log/device
        // { _id: 'xxxx' }
        protected Document Device()
        {
            var device = Server.GetDevice(Payload.ObjectId);
            return Success(device);
        }

        // history/log/day
        // { _id: 'xxxx', t: '2025-09-02' }
        protected Document Day()
        {
            Document res = Server.GetDayHistory(Payload.ObjectId, Payload.GetDateTime("t"), null);
            return Success(res);
        }
    }
}
