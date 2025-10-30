using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class BaseAccountController : Controller
    {
        protected virtual Document Login()
        {
            Memory.Account.WritePacket(RequestContext.MemoryPacket);
            return null;
        }
        protected virtual Document Logout()
        {
            return null;
        }
        protected virtual Document Response()
        {
            Screen.Warning($"Response : " + RequestContext.MemoryPacket.Document.ToString());
            return null;
        }
    }
}
