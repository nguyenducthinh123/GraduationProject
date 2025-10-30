using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp
{
    public class AppUser : Document
    {
        public ActionContext TopMenu { get; set; }
        public ActionContext SideMenu { get; set; }
        public object Profile { get; set; }
        public string Description { get; set; }
    }
}


namespace Actors
{
    public partial class Admin : Technical
    {
    }
    public partial class Technical : Actor
    {
        public override bool CanUpdateUser(string name) => true;
        public override bool CanUpdateDevice(string id) => true;
    }
    public partial class Customer : Actor
    {
        public override bool CanUpdateUser(string name)
        {
            bool b = true;
            GetStaffs(map => b = map.ContainsKey(name));

            return b;
        }
        public override bool CanUpdateDevice(string id)
        {
            bool b = true;
            GetDevices(map => b = map.ContainsKey(id));

            return b;
        }
    }
    public partial class Staff : Actor
    {
    }

    public partial class Guest : Actor
    {
    }
}

