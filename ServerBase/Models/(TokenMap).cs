using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class TokenMap : Dictionary<string, Document>
    {
        static public string Generate(string name) => name.JoinMD5(DateTime.Now);
        public Document Find(string token)
        {
            Document u = null;
            this.TryGetValue(token, out u);

            return u;
        }
    }
}
