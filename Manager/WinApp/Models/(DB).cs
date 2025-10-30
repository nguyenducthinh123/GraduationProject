using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BsonData;
using System.IO;

namespace System
{
    using PI = Vst.Server.ProcessInfo;
    using D = Models.Device;
    using BsonData;

    public class Slaves : Collection
    {
        public PI FindOne(string id) => Find<PI>(id);
        public void ForEach(Action<PI> callback)
        {
            foreach (var k in this.GetKeys())
            {
                var p = FindOne(k);
                callback(p);
            }
        }
        public Slaves(ServerDB db) : base("SERVERS", db)
        {
        }
        new public DocumentList SelectAll()
        {
            var lst = new DocumentList();
            ForEach(p => lst.Add(p));

            return lst;
        }

        static public string CreateSlavePath(string slavePath)
        {
            var v = Environment.CurrentDirectory.Split('\\');
            var s = slavePath.Split('\\');

            var l = new List<string>();
            var count = 0;
            while (l.Count < s.Length && l.Count < v.Length && v[count] == s[count])
                count++;

            for (int i = 0; i < v.Length - count; i++)
                l.Add("..");

            while (count < s.Length)
            {
                l.Add(s[count++]);
            }

            return string.Join("\\", l);
        }
    }

    public class Devices : Collection
    {
        public D FindOne(string id) => Find<D>(id);
        public Devices(ServerDB db) : base("DEVICES", db)
        {
        }

        public void UpdateDevice(string id, Document payload, Action<bool, D> callback)
        {
            var d = FindOne(id);
            if (d == null)
            {
                d = payload.ChangeType<D>();
                Insert(d);

                callback?.Invoke(true, d);
                return;
            }

            foreach (var p in payload)
            {
                d.Update(p.Key, p.Value);
            }
            callback?.Invoke(false, d);

            Update(d);
        }
    }

    public class ServerDB : Vst.Server.ServerDatabase
    {
        Slaves _slaves;
        public Slaves Slaves
        {
            get
            {
                if (_slaves == null)
                {
                    _slaves = new Slaves(this);
                }
                return _slaves;
            }
        }

        Accounts _accounts;
        public Accounts Accounts
        {
            get
            {
                if (_accounts == null)
                {
                    _accounts = new Accounts(this);
                }
                return _accounts;
            }
        }

        public Devices _devices;
        public Devices Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new Devices(this);
                }
                return _devices;
            }
        }

        public override void Connect(string connectionString)
        {
            base.Connect(connectionString);
            Config = LoadConfig("app_data/config");
        }
    }
}

