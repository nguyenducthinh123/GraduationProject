using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master
{
    class Payload
    {
        const byte open = (byte)'{';
        const byte close = (byte)'}';
        const byte quot = (byte)'"';
        const byte comma = (byte)',';

        List<byte[]> data = new List<byte[]>();
        public static bool CheckJsonFormat(byte[] value)
        {
            int o = 0, c = 0;
            foreach (var b in value)
            {
                switch (b)
                {
                    case open: ++o; break;
                    case close: ++c; break;
                }
            }
            return (o != 0 && o == c);
        }

        public Payload Add(byte[] payload)
        {
            data.Add(payload);
            return this;
        }
        public Payload Add(string payload)
        {
            return Add(payload.ASCII());
        }
        public Payload Add(string key, byte[] value)
        {
            var v = new byte[key.Length + value.Length + 1];
            int i = 0;
            foreach (var c in key)
            {
                v[i++] = (byte)c;
            }
            v[i++] = (byte)':';
            value.CopyTo(v, i);

            return Add(v);
        }
        public Payload Add(string key, string value)
        {
            var v = new byte[key.Length + value.Length + 3];
            int i = 0;
            foreach (var c in key)
            {
                v[i++] = (byte)c;
            }
            v[i++] = (byte)':';
            v[i++] = quot;

            foreach (var c in value)
            {
                v[i++] = (byte)c;
            }
            v[i] = quot;

            return Add(v);
        }
        public int Length
        {
            get
            {
                int n = 0;
                data.ForEach(r => n += r.Length);

                return n;
            }
        }
        public byte[] ToJSON()
        {
            var v = new byte[Length + 2 + data.Count - 1];
            var i = 0;
            
            v[i++] = open;
            data.ForEach(r => {
                r.CopyTo(v, i);
                i += r.Length;

                v[i++] = comma;
            });
            v[i - 1] = close;
            return v;
        }
    }
}
