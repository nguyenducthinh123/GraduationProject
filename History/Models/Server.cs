using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace History
{
    class Server : Vst.Server.SlaveServer<Database>
    {
        static public Device GetDevice(string id)
        {
            var device = MainDB.Devices.Find<Device>(id);
            if (device == null)
            {
                device = new Device();
                MainDB.Devices.Insert(id, device);
                MainDB.Devices.Wait();
            }
            return device;
        }
        static public Document GetDayHistory(string id, DateTime? date, Action<string, Document> callback)
        {
            var key = $"{id}-{date:yyMMdd}";
            Document doc = MainDB.EveryDay.Find(key);

            if (callback != null)
            {
                if (doc == null)
                {
                    MainDB.EveryDay.Insert(key, doc = new Document());
                    MainDB.EveryDay.Wait();
                }
                callback(key, doc);
            }
            return doc;
        }
    }
}
