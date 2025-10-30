using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace System
{
    public partial class Account : Document
    {
        #region Attributes
        #endregion

        public Account() { }
        public Account(string userName, string password)
        {
            var u = userName.ToLower();
            if (password != null)
            {
                password = u.JoinMD5(password);
            }
            ObjectId = u;
            Password = password;
        }
        public Account(string userName, string password, string role)
            : this(userName, password)
        {
            Role = role;
        }
        public virtual bool IsPasswordValid(string original, string encriped)
        {
            var epw = this.Password;
            if ((epw == null && UserName != original)
                || (epw != null && epw != encriped))
            {
                return false;
            }
            return true;
        }
    }
    public partial class Actor : Document
    {

        static public Document CreateRole(string role)
        {
            var t = Type.GetType($"Actors.{role}");
            if (t == null)
                return null;

            return (Document)Activator.CreateInstance(t);
        }
        public Document GetDevices(Action<Document> callback)
        {
            return SelectContext(nameof(Device), doc => callback(doc));
        }
        public Document GetStaffs(Action<Document> callback)
        {
            return SelectContext(nameof(Actors.Staff), doc => callback(doc));
        }
        public virtual bool CanUpdateUser(string name) => false;
        public virtual bool CanUpdateDevice(string id) => false;
    }
}

namespace System
{
    public class Accounts : BsonData.Collection
    {
        TokenMap _users = new TokenMap();
        public Accounts(ServerDB db) : base("ACCOUNTS", db) { }
        public Document FindLoggedIn(string token)
        {
            return _users.Find(token);
        }
        Document loginCore(string name, string pass)
        {
            var doc = Find(name);
            if (doc == null)
            {
                return new Document { Code = 1 };
            }
            Document acc = new Account(name, pass);
            if (acc.Password != doc.Password)
            {
                return new Document { Code = 2 };
            }

            acc = Actor.CreateRole(doc.Role);
            if (acc == null)
            {
                return new Document { Code = -1 };
            }

            acc.Copy(doc);
            return acc;
        }
        public Document TryLogin(string clientId, string name, string pass)
        {
            var acc = FindLoggedIn(clientId);
            if (acc != null)
                return acc;

            acc = loginCore(name, pass);
            if (acc.Code == 0)
            {
                _users.Add(clientId, (Actor)acc);
            }
            return acc;
        }
        public bool TryLogout(string clientId)
        {
            return _users.Remove(clientId);
        }
    }
}
