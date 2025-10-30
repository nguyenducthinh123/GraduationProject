using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Config
    {
        static public Document Main { get; private set; }
        static public Document Server { get; private set; }
        static public void Load(string path)
        {
            Func<string, Document> read = s => {
                using (var sr = new IO.StreamReader(path + s + ".json"))
                {
                    var text = sr.ReadToEnd();
                    return Document.Parse(text);
                }
            };

            Main = read("config");
            Actions = new ActionManager(read("actions"));
            Server = read("server");

            StartUrl = Main.GetString(nameof(StartUrl));
            Title = Main.GetString(nameof(Title));
            SubTitle = Main.GetString(nameof(SubTitle));
        }

        static public string StartUrl { get; private set; }
        static public string Title { get; private set; }
        static public string SubTitle { get; private set; }
        static public ActionManager Actions { get; private set; }
    }
}
