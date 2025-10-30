using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class Device : Document
    {
        static public List<string> GetDeviceInformationFields(Document pi)
        {
            List<string> items = new List<string>();

            var s = pi.GetString("fields");
            if (!string.IsNullOrWhiteSpace(s))
                foreach (var k in s.Split(';'))
                    items.Add(k.Trim());

            return items;
        }
        static public void RequestDevices(ServerBase server, params string[] fields)
        {
            List<string> items = new List<string>(fields);
            if (items.Count == 0)
            {
                items = GetDeviceInformationFields(server.ProcessInfo);
            }
            if (items.Count != 0)
            {
                server.SendInternalRequest("manager", null, "device/to-server", new Document { Fields = items });
            }
        }


        TodayLog _logs;
        public TodayLog Logs
        {
            get
            {
                if (_logs == null)
                {
                    _logs = new TodayLog(this);
                }
                return _logs;
            }
        }
    }

    public class DeviceCollecton<T> : BsonData.Collection
        where T: Device, new()
    {
        DocumentList _all;
        new public DocumentList SelectAll()
        {
            if (_all == null)
            {
                _all = new DocumentList();
                foreach (var k in GetKeys())
                    _all.Add(FindOne(k));
            }
            return _all;
        }

        public DeviceCollecton(BsonData.Database db) : base("DEVICES", db) { }
        public T FindOne(string id) => Find<T>(id);

        public void UpdateDevice(Document src, List<string> fields)
        {
            var id = src.ObjectId;
            var one = FindOne(id);
            if (one == null)
            {
                one = new T();
                one.Copy(src, fields);

                if (_all != null)
                    _all.Add(one);
                
                Insert(id, one);
            }
            else
            {
                UpdateCore(src.Move(one, fields));
            }
        }
        public void RemoveDevice(string id)
        {
            FindAndDelete(id, doc => {
                if (_all != null)
                    _all.Remove(doc);
            });
        }
    }
}
