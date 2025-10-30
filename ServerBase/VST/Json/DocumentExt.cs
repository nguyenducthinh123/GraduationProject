
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public partial class Document
    {
        #region Request

        #endregion

        public string Url { get { return GetString("url"); } set => Push("url", value); }
        public string Action { get { return GetString("action"); } set => Push("action", value); }
        public string Name { get { return GetString("name"); } set => Push("name", value); }

        public object Value
        {
            get
            {
                TryGetValue("value", out object v);
                return v;
            }
            set => Push("value", value);
        }
        public int Code { get { return GetValue<int>("code"); } set => Push("code", value); }
        public string Message { get { return GetString("message"); } set => Push("message", value); }

        public int GetActionCode()
        {
            return Pop<int>("code");
        }

        #region VALUES
        public Document ValueContext => SelectContext("value", v => { });
        public T GetValue<T>() => (T)Convert.ChangeType(Value, typeof(T));
        public static NameMapping NameMapping { get; private set; } = new NameMapping();
        public object GetObject(string name, bool ignoreCase)
        {
            if (ignoreCase)
            {
                name = NameMapping[name];
                if (name == null)
                {
                    return null;
                }
            }
            TryGetValue(name, out object v);
            return v;


        }
        #endregion
    }

    public class NameMapping : Dictionary<string, string>
    {
        new public string this[string key]
        {
            get
            {
                key = key.ToLower();
                string value;
                TryGetValue(key, out value);

                return value;
            }
            set
            {
                key = key.ToLower();
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    base.Add(key, value);
                }
            }
        }
        public void Add(string key)
        {
            base.Add(key.ToLower(), key);
        }
        public void Add(object obj)
        {
            Add(JObject.ToDocument(obj));
        }
        public void Add(Document doc)
        {
            foreach (var key in doc.Keys)
            {
                var k = key.ToLower();
                if (base.ContainsKey(k) == false)
                {
                    base.Add(k, key);
                }
            }
        }
        new public bool Remove(string key)
        {
            return base.Remove(key.ToLower());
        }
    }
}
