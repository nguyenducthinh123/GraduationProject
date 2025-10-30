using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Controllers
{
    using Vst.Server;
    class BaseController : Controller
    {
        public Actor User => RequestContext.User;
        public AccountCollection Accounts => AccountServer.DB.Accounts;

        public void TryUpdateUser(string name, Action<Document> callback)
        {
            if (User.CanUpdateUser(name ?? ValueContext.UserName))
                callback(ValueContext);
        }
        public void TryUpdateDevice(string id, Action<Document> callback)
        {
            if (User.CanUpdateDevice(id))
                callback(ValueContext);
        }

        public void SetRole(Document doc, string service, object role)
        {
            doc.SelectContext(nameof(Document.Role), s => s.Push(service, role));
        }
        public Document CreateOne(string service, Document doc)
        {
            var id = doc.UserName;
            var role = doc.Pop<string>(nameof(Document.Role));

            Document acc = null;
            Action<Document> set = (a) => {
                SetRole(acc = a, service, role);
            };

            Accounts.FindAndUpdate(id, a => {
                set(a);
            });
            if (acc == null)
            {
                var ps = doc.Password;
                if (string.IsNullOrEmpty(ps))
                {
                    var i = id.Length - 1;
                    if (i < 0) i = 0;

                    ps = id.Substring(i);
                }
                set(new System.Account(id, ps));
                Accounts.Insert(acc);
            }
            return acc;
        }
        public void RemoveOne(string id)
        {
            Stack<string> keys = new Stack<string>();
            keys.Push(id);

            while (keys.Count != 0)
            {
                id = keys.Pop();
                Accounts.FindAndDelete(id, acc => {
                    acc.SelectContext("staff", s => {
                        foreach (var k in s.Keys)
                        {
                            keys.Push(k);
                        }
                    });
                });
            }
        }
    }
}
