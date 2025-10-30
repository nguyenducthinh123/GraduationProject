using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    using System.IO;
    public class Config : Document
    {
        static public Document Load(string path, string name)
        {
            var fn = $"{path}/{name}.json";
            if (File.Exists(fn))
            {
                 return Parse(File.ReadAllText(fn));
            }
            return new Document();
        }
        static public bool Save(string path, string name, Document content)
        {
            if (!Directory.Exists(path)) return false;
            File.WriteAllText($"{path}/{name}.json", content.ToString());

            return true;
        }
    }
}
