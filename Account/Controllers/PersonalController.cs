using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Controllers
{
    using Vst.Server;
    class PersonalController : BaseController
    {
        // account/personal/login
        // { UserName: '0989154248', Password: '1234' }
        //protected virtual Document Login()
        //{
        //    RequestContext.Uri[2] = "response";

        //    var acc = Accounts.Find(Payload.ObjectId);
        //    if (acc == null)
        //    {
        //        return Response(new ErrorContext("NOTFOUND"), false);
        //    }
        //    var role = acc.GetDocument(nameof(Document.Role)).GetString(RequestContext.ServerName);
        //    if (role == null)
        //    {
        //        return Response(new ErrorContext("NOSERVICE"), false);
        //    }
        //    var token = TokenMap.Generate(acc.ObjectId);
        //    acc.Token = token;

        //    var type = Type.GetType($"Actors.{role}");
        //    if (type == null)
        //    {
        //        type = typeof(Guest);
        //    }
        //    var actor = (Actor)Activator.CreateInstance(type);
        //    actor.BD = DateTime.Now;
        //    actor.ObjectId = acc.ObjectId;

        //    Users.Add(token, actor);

        //    return Response(acc, false);
        //}
        // account/personal/logout
        // { token: 'xxxx' }
        protected virtual Document Logout()
        {
            AccountServer.DB.Users.Remove(RequestContext.ClientId);
            return Success();
        }

        // account/personal/change-password
        // { token: 'xxxx', value: { Password: '1234' } }
        protected virtual Document ChangePassword()
        {
            var name = User.ObjectId;
            Accounts.FindAndUpdate(name, doc => {
                var acc = new System.Account(name, ValueContext.Password);
                doc.Password = acc.Password;
            });
            return Success();
        }
    }
}
