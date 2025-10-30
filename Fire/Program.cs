using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start();

            Device.Processor = server;

            var id = "0000000006";
            Device demo = null;

            Screen.Loop((name, param) => {
                try
                {
                    demo = Device.FindOne(id);
                    switch (name)
                    {
                        case "d":
                        case "w":
                        case "f":
                            demo.Simulate(name);
                            break;

                        case "on":
                        case "off":
                            Task.Run(async () => {
                                var rd = demo.Reader;
                                await rd.OnOff(id, name.ToUpper());

                                var t = DateTime.Now;
                                var lst = new DocumentList();
                                while (true)
                                {
                                    System.Threading.Thread.Sleep(1000);

                                    lst = await rd.GetHistory(id, t.AddSeconds(-1), t);
                                    if (lst != null && lst.Count > 0)
                                    {
                                        foreach (var e in lst)
                                            Console.WriteLine($"Relay = {e.GetString("Relay")}");
                                        break;
                                    }
                                }
                            });
                            break;

                        case "r":
                            Device.RequestDevices(server);
                            break;

                        case "i":
                            Screen.Info(demo.ToString());
                            break;

                        case "s":
                            Screen.Info(new Document { { "items", demo.GetRecords() } }.ToString());
                            break;

                        case "demo":
                            id = param[0];
                            break;

                        case "edit":
                            server.Publish("manager/device/update", new Document {
                                ObjectId = demo.ObjectId,
                                Value = new Document { { param[0], param[1] } }
                            });
                            break;
                    }

                }
                catch (Exception e)
                {

                }
            });
        }
    }
}
