using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire.Controllers
{
    class RemoteController : BaseController
    {
        void Remote(string action)
        {
            Task.Run(async () => {
                var data = RequestContext.Payload;
                var id = data.ObjectId;
                var device = Device.FindOne(id);

                data.Action = action;
                await device.OnOff(data);
            });
        }
        protected Document On()
        {
            Remote("ON");
            return null;
        }

        protected Document Off()
        {
            Remote("OFF");
            return null;
        }
    }
}
