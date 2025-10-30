using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers.Internal
{
    class AccountController : BaseInternalController
    {
        #region ME
        // manager/account/login
        // { userName: '0989154248', password: '1234' }
        protected virtual Document Login()
        {
            var acc = App.Manager.AccountCollection.TryLogin(RequestContext.ClientId, Payload.UserName, Payload.Password);
            if (acc.Code != 0)
                return acc;

            App.Manager.CreateMessage($"Logged in: {acc.Name}");
            return Success(acc);
        }

        // manager/account/logout
        // { }
        protected virtual Document Logout()
        {
            App.Manager.AccountCollection.TryLogout(RequestContext.ClientId);
            return Success();
        }

        // manager/account/change-password
        // { password: '1234' }
        protected virtual Document ChangePassword()
        {
            return UpdateAccount(doc => {
                    var acc = new System.Account(doc.ObjectId, Payload.Password);
                    doc.Password = acc.Password;
                });
        }

        // manager/account/uppdate-profile
        // { xxx }
        protected virtual Document UpdateProfile()
        {
            return UpdateAccount(doc => {
                doc.Push("me", Payload);
                doc.Name = Payload.Name;
            });
        }
        #endregion


        #region User

        DocumentList GetUsers(Document doc)
        {
            var lst = new DocumentList();
            if (doc != null)
            {
                foreach (var p in doc)
                {
                    var d = App.Manager.AccountCollection.Find(p.Key);
                    if (d != null)
                    {
                        lst.Add(d);
                    }
                }
            }
            return lst;
        }

        // manager/account/user-list
        protected Document UserList()
        {
            return GetActor(a => {
                var res = new Document();
                a.GetStaffs(doc => res.Items = GetUsers(doc));

                return res;
            });
        }

        // manager/account/add-user
        // { userName: '', role: '', name: '' }
        protected Document AddUser()
        {
            return GetActor(a => {
                var name = Payload.UserName;
                if (Accounts.Find(name) != null)
                {
                    return new Document { Code = 1 };
                }

                var i = name.Length - 4;
                if (i < 0) i = 0;
                var pass = name.Substring(i);
                var acc = new Account(name, pass, Payload.Role);
                acc.Name = Payload.Name;

                Accounts.Insert(name, acc);

                if (a is Actors.Customer)
                {
                    a.GetStaffs(doc => doc.Push(name, new Document()));
                    Accounts.Update(a);
                }

                return Success(new Document { 
                    ObjectId = acc.ObjectId,
                    Password = pass,
                });
            });
        }

        // manager/account/remove-user
        // { _id: '' }
        protected Document RemoveUser()
        {
            return GetActor(a => {

                var id = Payload.ObjectId;
                if (a.CanUpdateUser(id))
                {                    
                    a.GetStaffs(doc => doc.Remove(id));
                    Accounts.Delete(id);
                    Accounts.Update(a);
                }
                return Success();
            });
        }

        #endregion

        #region Device

        DocumentList GetDevices(Document doc)
        {
            var lst = new DocumentList();
            if (doc != null)
            {
                foreach (var p in doc)
                {
                    var d = App.Manager.DevicesCollection.Find(p.Key);
                    if (d != null)
                    {
                        lst.Add(d);
                    }
                }
            }
            return lst;
        }

        // manager/account/device-list
        protected Document DeviceList() 
        {
            var res = new Document();
            return GetActor(a => {
                a.GetDevices(doc => res.Items = GetDevices(doc));

                return res;
            });
        }
        // manager/account/device-to-user
        // { userId: '', deviceId: '', action: '+/-' }
        protected Document DeviceToUser()
        {
            return GetActor(a => {

                var userId = Payload.GetString("userId");
                var deviceId = Payload.GetString("deviceId");
                if (a.CanUpdateUser(userId) && a.CanUpdateDevice(deviceId))
                {
                    Manager.CreateMessage(null, $"{a.Name} set device to user").SetContent(Payload);
                    Manager.AccountCollection.FindAndUpdate(userId, u => {
                        u.SelectContext("device", doc => { 
                            if (Payload.Action == "+")
                            {
                                doc.Push(deviceId, new Document());
                            }
                            else
                            {
                                doc.Remove(deviceId);
                            }
                        });
                    });
                }
                return Success();
            });
        }
        #endregion
    }
}
