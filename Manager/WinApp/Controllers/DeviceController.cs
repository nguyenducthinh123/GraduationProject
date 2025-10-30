using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers
{
    class DeviceController : BaseController
    {
        static public Models.Device Current { get; set; }
        public override object Index()
        {
            return View(Current);
        }
        public object Index(string id)
        {
            var d = Manager.DevicesCollection.FindOne(id);
            if (d == null) return null;

            return View(Current = d);
        }
    }
}
