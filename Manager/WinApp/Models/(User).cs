using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    using Actors;
    using BsonData;

    public class TokenMap : Dictionary<string, Actor>
    {
        public Actor Find(string token)
        {
            Actor u = null;
            this.TryGetValue(token, out u);

            return u;
        }
    }

    //partial class DB
    //{
    //    static Collection _accounts;
    //    public static Collection Accounts
    //    {
    //        get
    //        {
    //            if (_accounts == null)
    //            {
    //                _accounts = Main.GetCollection(nameof(Accounts));
    //            }
    //            return _accounts;
    //        }
    //    }

    //    static TokenMap _users;
    //    static public TokenMap Users
    //    {
    //        get
    //        {
    //            if (_users == null)
    //            {
    //                _users = new TokenMap();
    //            }
    //            return _users;
    //        }
    //    }
    //}
}
