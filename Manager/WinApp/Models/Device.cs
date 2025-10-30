using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PhoneNumbers : Document
    {
        public void SetNumbers(string numbers, long flag)
        {
            var doc = new Document();
            foreach (string k in Keys)
            {
                var v = (long)this[k];
                v &= ~flag;
                if (v != 0)
                {
                    doc.Add(k, v);
                }
            }    
            foreach (var s in numbers.Split(';'))
            {
                var n = s.Trim();
                if (n != string.Empty)
                {
                    if (doc.TryGetValue(n, out object v)) {
                        doc[n] = (long)v | flag;
                    }
                    else
                    {
                        doc.Add(n, flag);
                    }
                }
            }

            Clear();
            Copy(doc);
        }
    }
    public class Device : Vst.Server.Device
    {
        public PhoneNumbers Phone => SelectContext<PhoneNumbers>("phone", e => { });
        public void Update(string key, object value)
        {
            if (key == "smsTo")
            {
                Phone.SetNumbers((string)value, 1);
                Remove(key);
                return;
            }
            if (key == "callTo")
            {
                Phone.SetNumbers((string)value, 2);
                Remove(key);
                return;
            }    

            Push(key, value);
        }
    }
}
