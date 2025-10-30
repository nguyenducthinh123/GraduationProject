using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using VS = Vst.Server;

namespace WinApp
{
}

namespace WinApp.Controllers
{
    using PI = VS.ProcessInfo;
    class ServerController : BaseController
    {
        public static PI Current { get; set; }
        public override object Index()
        {
            return View(new ServersModel());
        }

        public object Open(PI p) => View(Current = p);
        public object Add() => View();
        public object Add(string path)
        {
            var p = new PI { 
                FullPath = path,
            };

            p.Path = Slaves.CreateSlavePath(p.Path);
            App.Request("server/edit", p);
            return null;
        }
        public object Edit(PI p)
        {
            return View(new EditContext(Current = p));
        }
        public object Delete(PI p)
        {
            p.Stop();
            //VS.ShareMemory.Open(p.ObjectId, sm => sm.Dispose());
            
            Slaves.Delete(p);
            return RedirectToAction("index");
        }
        public object Update(EditContext context)
        {
            var p = (PI)context.Model;
            var e = Slaves.FindOne(p.ObjectId);

            p.Stop();
            if (e == null)
            {
                Slaves.Insert(p);
            }
            else
            {
                Slaves.Update(p);
            }

            p.SaveConfig();

            App.RedirectToAction("open", p);
            return null;
        }
        public object Show(string name)
        {
            var p = Slaves.FindOne(name);
            p.Hidden = false;

            App.RedirectToAction("open", p);
            return null;
        }
        public object Broker()
        {
            return View();
        }
        public object SetPath(string path)
        {
            Current.FullPath = path;

            Current.Path = Slaves.CreateSlavePath(Current.Path);
            Slaves.Update(Current);

            App.RedirectToAction("index");
            return null;
        }
    }
}
