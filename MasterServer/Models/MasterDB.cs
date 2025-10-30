using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vst.Server;
namespace Master
{
    using PI = ProcessInfo;
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
        public Slaves(DB db) : base("SERVERS", db)
        {
        }
        public void StartOne(PI p)
        {
            if (Config.Save(p.Path, p.ObjectId, p))
            {
                p.Start();
            }
        }

        new public DocumentList SelectAll()
        {
            var lst = new DocumentList();
            ForEach(p => lst.Add(p));

            return lst;
        }
    }
    public class DB : ServerDatabase
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
    }
}
