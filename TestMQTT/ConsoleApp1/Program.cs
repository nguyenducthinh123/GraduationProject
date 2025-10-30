using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Vst.MQTT;

namespace TestMQTT
{
    class Program
    {
        static void TestMQTT()
        {
            const string host = "localhost";
            //const string host = "broker.emqx.io";
            const string topic = "0000000006/status";
            Client client = new Client(host);

            client.ConnectionError += () => Screen.Error("Fail");
            client.Disconnected += () => Screen.Warning("Connection Lost");
            client.Connected += () => {
                Screen.Done();
                client.Subscribe(topic);
            };
            client.DataReceived += (t, p) => {
                try
                {
                    var message = p.UTF8();
                    Screen.Info(message);
                }
                catch
                {
                }
            };

            Screen.Message($"Connect to the MQTT server {host} ... ");
            client.Connect();

            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == "conn")
                {
                    client.Connect();
                    continue;
                }
                if (msg == "disc")
                {
                    client.Disconnect();
                    continue;
                }
                client.Publish(topic, msg);
            }
        }

        static void TestMqttClient()
        {

            const string topic = "vst_test_mqtt";
            var m = new Client("Localhost") { };
            m.Connected += () => { 
                Screen.Success($"{m.Host} connected");
                m.Subscribe(topic);
            };
            m.ConnectionError += () => Screen.Error($"Connect to {m.Host} fail");
            m.ConnectionLost += () => {
                Screen.Error("Connection lost");
            };
            m.Disconnected += () => Screen.Warning($"{m.Host} disconnected");
            m.DataReceived += (t, p) => {
                Screen.Info(p.UTF8());
            };

            m.SetCheckConnectionInterval(1);

            m.Connect();

            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "c": m.Connect(); break;
                    case "d": m.Disconnect(); break;
                    default:
                        m.Publish(topic, cmd);
                        break;
                }
            }
        }

        static void TestManager()
        {
            var DB = new BsonData.MainDatabase("MainDB");
            DB.Connect(Environment.CurrentDirectory);

            var demo = new Document { ObjectId = "0000000006" };

            //var q = new Client("system.aks.vn") { ID = $"test_mqtt_{DateTime.Now.Ticks}" };
            var q = new Client("localhost") { ID = $"test_mqtt_{DateTime.Now.Ticks}" };
            q.SetCheckConnectionInterval(3);

            Action<string, object> send = (a, o) => {
                q.Publish($"manager/{a}", o?.ToString() ?? "{}");
            };
            Action<string, object> history = (a, o) => {
                q.Publish($"history/{a}", o?.ToString() ?? "{}");
            };
            Action login = () => {
                send("account/login", new Document { UserName = "0902186628", Password = "6628" });
            };

            Queue<Document> records = new Queue<Document>();
            q.DataReceived += (t, p) => {
                var msg = p.UTF8();
                var res = Document.Parse(msg);
                if (res.Url == "device/getstatus")
                {
                    var items = res.ValueContext.Items;
                    if (items?.Count > 0)
                    {
                        foreach (var r in items)
                        {
                            records.Enqueue(r);
                        }    
                    }

                    return;
                }


                Screen.Warning(t);
                Screen.Info(msg);
            };
            q.Connected += () => {
                Screen.Success($"{q.Host} connected");
                q.Subscribe("response/" + q.ID);
                q.Subscribe("alarm/" + demo.ObjectId);
            };

            DateTime? last = null;
            int requestStatus = 0;

            Action getStatus = () => {

                if (records != null && records.Count > 0)
                {
                    var r = records.Dequeue();
                    Screen.Info(r.ToString());
                }    

                if (requestStatus == 0)
                    return;

                if (records.Count == 0)
                {
                    var doc = new Document { ObjectId = demo.ObjectId };
                    if (last != null) doc.Add("t", last);

                    q.Publish("FIRE/device/get-status", doc.ToString());
                }    

            };

            q.Connect();

            var clock = new Clock();
            clock.OneSecond += getStatus;

            clock.StartAsync();

            Screen.Loop((name, param) =>
            {
                switch (name)
                {
                    case "login":
                        login();
                        break;

                    case "logout":
                        send("account/logout", null);
                        break;


                    case "d":
                        send("account/device-list", null);
                        break;

                    case "u":
                        send("account/user-list", null);
                        break;

                    case "on":
                        demo.Action = "On";
                        send("device/remote", demo);
                        break;

                    case "off":
                        demo.Action = "Off";
                        send("device/remote", demo);
                        break;

                    case "edit":
                        demo.Value = new Document {
                            { param[0], param[1] }
                        };
                        send("device/update", demo);
                        break;

                    case "log":
                        history("log/device", demo);
                        break;

                    case "daylog":
                        demo.Push("t", Screen.GetParam(0) ?? DateTime.Now.ToString("yyyy-MM-dd"));
                        history("log/day", demo);
                        break;

                    case "status":
                        requestStatus = 3;
                        getStatus();
                        break;

                    case "create-user":
                        send("account/add-user", new Document {
                            { "userName", "0989136384" },
                            { "name", "Huwng" },
                            { "role", "Staff" },
                        });
                        break;

                    case "d2u":
                        send("account/device-to-user", new Document {
                            { "deviceId", demo.ObjectId },
                            { "userId", "0989136384" },
                            { "action", "+" }
                        });
                        break;
                }
            });
        }

        static void Main(string[] args)
        {
            TestManager();

            while (true) { }
        }
    }
}
