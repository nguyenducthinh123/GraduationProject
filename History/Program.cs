using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace History
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();

            const string defaultId = "0000000006";
            string id = defaultId;
            while (true)
            {
                var items = Console.ReadLine().Split(' ');
                var cmd = items[0];

                Func<int, string, string> p = (i, s) => {
                    if (i >= items.Length) return s;
                    return items[i];
                };

                try
                {
                    switch (cmd)
                    {
                        case "login":
                            server.Publish("manager/account/login", new Document
                            {
                                UserName = p(1, "0902186628"),
                                Password = p(2, "6628"),
                            });
                            break;

                        case "demo":
                            id = p(1, defaultId);
                            break;

                        case "edit":
                            server.Publish("manager/device/update", new Document {
                                ObjectId = id,
                                Value = new Document { { p(1, "iDanger"), p(2, "100") } }
                            });
                            break;
                    }

                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
