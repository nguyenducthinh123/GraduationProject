using BsonData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    //public class AccountCollection : Collection
    //{
    //    public AccountCollection(Database db) : base("Accounts", db) { }
    //    public int TryLogin(string un, string ps, Action<Document> callback)
    //    {
    //        var e = Find(un);
    //        if (e == null)
    //        {
    //            return -1;
    //        }
    //        var o = new Account(un, ps);
    //        if (o.Password != e.Password)
    //        {
    //            return 1;
    //        }

    //        e.Token = TokenMap.Generate(un);
    //        callback(e);

    //        return 0;
    //    }
    //}
}
