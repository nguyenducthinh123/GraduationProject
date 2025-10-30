using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers.Internal
{
    class BaseInternalController : Vst.Server.Controller
    {
        static public Models.Manager Manager => App.Manager;
        static public Accounts Accounts => Manager.AccountCollection;

        public Document GetActor(Func<Actor, Document> callback)
        {
            var act = (Actor)Accounts.FindLoggedIn(RequestContext.ClientId);
            if (act == null) return null;

            return callback(act);
        }
        public Document UpdateAccount(Action<Document> callback)
        {
            return GetActor(a => {
                callback(a);
                Accounts.Update(a);

                return Success();
            });
        }
    }
}
