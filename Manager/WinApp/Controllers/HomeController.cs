using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers
{
    class HomeController : BaseController
    {
        void LoadActions()
        {
            var u = App.User;
            if (u != null)
            {
                var name = u.Role;
                var top = u.TopMenu;
                if (top == null)
                {
                    u.TopMenu = top = Config.Actions.GetTopMenu(name);
                }
                u.SideMenu = new ActionContext { Childs = top.Childs[0].Childs };
            }

        }
        public override object Index()
        {
            LoadActions();
            if (Manager.Client == null)
            {
                App.Request("home/loading", new Models.ServersModel());
                return null;
            }
            return View(Manager);
        }
        public object Loading(Models.ServersModel model)
        {
            return View(model);
        }
        public object Ready()
        {
            LoadActions();

            return RedirectToAction("Index");
        }
        public object Admin() => Redirect("master");
    }
}
