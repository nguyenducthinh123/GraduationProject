using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire.Controllers
{
    class DeviceController : BaseController
    {
        // fire/device/get-status
        // { _id: 'xxxx', t: '2025-09-02 15:00:30' }
        protected Document GetStatus()
        {
            var device = Device.FindOne(Payload.ObjectId);
            if (device == null)
                return Error(-1);

            //var items = device.Records.Select(Payload.GetDateTime("t"), 10);
            
            return Success(new Document { { "items", device.GetRecords() } });
        }

        // fire/device/status
        // { _id: 'xxxx', t: '2025-09-02 15:00:30' }
        protected Document Status()
        {
            var device = Device.FindOne(Payload.ObjectId);
            if (device == null)
                return Error(-1);

            //var items = device.Records.Select(Payload.GetDateTime("t"), 10);

            return Success(new Document { { "items", device.GetRecords() } });
        }




        protected Document Update()
        {
            CheckManager(() => {
                foreach (var e in Payload.Items)
                {
                    Device.DB.UpdateDevice(e, Device.GetDeviceInformationFields(Processor.ProcessInfo));
                }
            });
            return null;
        }
    }
}
