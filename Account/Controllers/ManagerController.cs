using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.Controllers
{
    class ManagerController : BaseController
    {
        // account/manager/create
        // { token: 'xxxx', value: { UserName: '0989154248', Password: '1234', Role: 'Technical' } }
        protected virtual Document Create()
        {
            TryUpdateUser(null, v => CreateOne(RequestContext.ServerName, v));
            return Success();
        }
        // account/manager/remove
        // { token: 'xxxx', value: { UserName: '0989154248' } }
        protected virtual Document Remove()
        {
            var un = ValueContext.UserName;
            TryUpdateUser(un, v => RemoveOne(un));
            return Success();
        }

        // account/manager/set-role
        // { token: 'xxxx', value: { UserName: '0989154248', Role: 'Technical' } }
        protected virtual Document SetRole()
        {
            var name = ValueContext.UserName;
            TryUpdateUser(name, v => {
                Accounts.FindAndUpdate(name, doc => {
                    SetRole(doc, RequestContext.ServerName, v.Role);
                });
            });
            return Success();
        }

        // account/manager/add-staff
        // { token: 'xxxx', value: { UserName: '0989154248', Password: '1234' } }
        protected virtual Document AddStaff()
        {
            ValueContext.Role = nameof(Actors.Staff);
            var acc = CreateOne(RequestContext.ServerName, ValueContext);

            Accounts.FindAndUpdate(User.ObjectId, e => {
                e.GetStaffs(map => {
                    map.Push(acc.ObjectId, new Document());
                });
            });
            return Success();
        }

        // account/manager/remove-staff
        // { token: 'xxxx', value: { UserName: '0989154248' } }
        protected virtual Document RemoveStaff()
        {
            Accounts.FindAndUpdate(User.ObjectId, e => {
                e.GetStaffs(map => {
                    var k = ValueContext.UserName;
                    if (map.Remove(k))
                    {
                        RemoveOne(k);
                    }
                });
            });
            return Success();
        }
        void RemoveDeviceFromAccount(string un, string de)
        {
            Accounts.FindAndUpdate(un, doc => {
                doc.GetDevices(d => d.Remove(de));
                doc.GetStaffs(map => {
                    foreach (var k in map.Keys)
                    {
                        RemoveDeviceFromAccount(k, de);
                    }
                });
            });
        }
        // account/manager/device-mapping
        // { token: 'xxxx', action: '+/-', value: { UserName: '0989154248', Device: '0001', value: {} } }
        protected Document DeviceMapping()
        {
            var un = ValueContext.UserName;
            var de = ValueContext.Device;
            TryUpdateUser(un, doc => {
                if (User.CanUpdateDevice(doc.Device))
                {
                    var v = ValueContext.Value;
                    if (Payload.Action != "-")
                    {
                        Accounts.FindAndUpdate(un, acc => {
                            acc.GetDevices(d => d.Push(de, v ?? new Document()));
                        });
                    }
                    else
                    {
                        RemoveDeviceFromAccount(un, de);
                    }
                }
            });
            return Success();
        }
    }
}
