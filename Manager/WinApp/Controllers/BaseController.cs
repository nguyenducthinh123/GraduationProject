using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers
{
    using Models;
    class UpdateContext : EditContext
    {
        public string Message { get; set; }
    }
    class BaseController : System.Mvc.Controller
    {
        public Master Master => Manager.Master;
        public Manager Manager => App.Manager;
        public Slaves Slaves => Manager.SlavesCollection;

        public virtual object Index() => View();
    }
}
