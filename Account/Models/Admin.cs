using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors
{
    public class Admin : Actor
    {
        public override bool CanUpdateUser(string name) => true;
        public override bool CanUpdateDevice(string id) => true;
    }
    public class Customer : Actor
    {
        public override bool CanUpdateUser(string name) => true;
        public override bool CanUpdateDevice(string id) => true;
    }
}
