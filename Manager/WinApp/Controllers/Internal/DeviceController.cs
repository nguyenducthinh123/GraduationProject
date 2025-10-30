using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Controllers.Internal
{
    using Models;
    class DeviceController : BaseInternalController
    {
        void SendDeviceInfo(string name, List<string> fields, IEnumerable<Document> items, bool checkEmpty)
        {
            if (fields == null)
            {
                var pi = Manager.SlavesCollection.Find(name);
                fields = Device.GetDeviceInformationFields(pi);

                if (checkEmpty && fields.Count == 0)
                    return;
            }

            var lst = new DocumentList();
            foreach (var e in items)
            {
                var o = new Document().Copy(e, fields);
                if (checkEmpty && o.Count == 0)
                    continue;

                o.ObjectId = e.ObjectId;
                lst.Add(o);
            }

            if (lst.Count != 0) Manager.SendDeviceInfo(name, lst);
        }

        // manager/device/connect
        // { _id: 'xxx', model: 'xxx', version: 'xxx' } }
        protected Document Connect()
        {
            if (Manager.DevicesCollection.Insert(Payload))
            {
                var model = Payload.Model;
                SendDeviceInfo(model, null, new DocumentList { Payload }, false);
            }

            var t = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var res = new Document {
                Token = Payload.ObjectId.JoinMD5(t),
            };
            res.Add("time", t);
            return res;
        }


        // manager/device/update
        // { _id: 'xxx', value: { xxx } }
        protected Document Update()
        {
            return GetActor(a => {
                var id = Payload.ObjectId;
                var model = string.Empty;
                if (a.CanUpdateDevice(id))
                {
                    Manager.DevicesCollection.UpdateDevice(id, ValueContext, (isNew, doc) => {
                        
                        model = doc.Model;
                        if (!string.IsNullOrEmpty(model))
                        {
                            SendDeviceInfo(model, null, new DocumentList { doc }, true);
                        }
                        Manager.CreateMessage($"{a.Name} update device {id}").SetContent(doc.ToString());
                    });

                }
                return Success();
            });
        }
        
        // manager/device/remote
        // { _id: 'xxx', action: 'xxx', value: { 'xxx' } }
        protected Document Remote()
        {
            return GetActor(a => {
                var id = Payload.ObjectId;
                if (a.CanUpdateDevice(id))
                {
                    var device = Manager.DevicesCollection.FindOne(id);
                    ValueContext.ObjectId = id;
                    Manager.SendInternalRequest(device.Model, RequestContext.ClientId, $"remote/{Payload.Action}", ValueContext);
                }
                return null;
            });
        }
        protected Document Alarm()
        {
            var id = RequestContext.ClientId;
            var device = Manager.DevicesCollection.FindOne(id);
            if (device == null)
                return null;

            var phones = new Document();
            Func<string, int, int> set = (n, v) => {
                var s = device.GetString(n);
                if (s == null) return 0;

                foreach (var k in s.Split(';'))
                {
                    var f = phones.GetValue<int>(k);
                    phones.Remove(k);
                    phones.Add(k, f | v);
                }
                device.Remove(n);
                return 1;
            };

            var b = set("smsTo", 1) + set("callTo", 2);
            if (b != 0)
            {
                device.Push("phone", phones);
                Manager.DevicesCollection.Update(device);
            }
            else
            {
                phones = device.SelectContext("phone", null);
            }

            foreach (var n in phones.Keys)
            {
                var f = phones.GetValue<int>(n);
                if ((f & 1) != 0)
                    Sim.DefaultChannel.CreateSMS(n, Payload.Message);

                if ((f & 2) != 0)
                    Sim.DefaultChannel.CreateCALL(n);
            }

            return null;
        }
        protected Document ToServer()
        {
            var fields = Payload.Fields;
            var name = RequestContext.ServerName.ToUpper();

            SendDeviceInfo(name, fields, Manager.DevicesCollection.Select(x => x.Model == name), false);
            return null;
        }
    }
}
