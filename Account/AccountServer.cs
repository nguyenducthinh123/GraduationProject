using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vst;
using Vst.Server;

namespace Account
{
    using Acc = System.Account;
    class AccountDB : ServerDatabase
    {
        public TokenMap Users { get; private set; } = new TokenMap();
        public AccountCollection Accounts { get; private set; }
        public AccountDB()
        {
            Accounts = new AccountCollection(this);
        }
    }
    class AccountServer : SlaveServer<AccountDB>
    {
        Actor GetActor(RequestContext context)
        {
            var user = (Actor)DB.Users.Find(context.ClientId);
            if (user == null)
            {
                var un = context.Payload.UserName;
                var ps = context.Payload.Password;
                if (un == null || ps == null)
                {
                    return null;
                }

                int code = DB.Accounts.TryLogin(un, ps, e => {
                    var role = e.Role;
                    if (role == null)
                    {
                        ProcessResponse(context, Document.Error("NOSERVICE"));
                        return;
                    }

                    var type = Type.GetType($"Actors.{role}");
                    if (type == null)
                    {
                        type = typeof(Actors.Guest);
                    }
                    user = (Actor)Activator.CreateInstance(type);
                    user.Copy(e);
                    user.LastAccess = DateTime.Now;

                    DB.Users.Add(context.ClientId, user);
                    return;
                });
                if (user != null)
                {
                    user.Password = null;
                    ProcessResponse(context, Document.Success(user));
                }
                else
                {
                    ProcessResponse(context, Document.Error("LOGINERROR"));
                }
                return null;
            }
            return user;
        }
        protected override void ProcessRequest(RequestContext context)
        {
            var actor = GetActor(context);
            if (actor != null)
            {
                //if (actor.Token != context.Payload.Token)
                //{
                //    ProcessResponse(context, Document.Error("TOKEN"));
                //    return;
                //}

                var cname = context.ControllerName.ToCodeName();
                cname = DB.Config.GetString(cname);

                if (cname != null)
                {
                    Screen.Warning($"Found internal action: {cname}");

                    context.Uri[0] = "#";
                    context.Uri[1] = cname;

                    var mp = new MemoryPacket(context.Uri.ToString(), context.Payload.ValueContext.ToString().UTF8());
                    //var sm = Memory.Firmware;
                    //Screen.Waiting($"Write internal action: {sm.Name}", 1, () => sm.WritePacket(mp));

                    return;
                }
                context.User = actor;
                base.ProcessRequest(context);
            }
        }
    }
}
