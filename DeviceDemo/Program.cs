using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Vst.MQTT.Client("broker.emqx.io");
            client.Connected += () => {
                Screen.Success("MQTT connected");
            };
            client.SetCheckConnectionInterval(3);

            const string topic = "device/status/0000000003";
            var rand = new Random();

            Task.Run(async () => { 
                while (true)
                {
                    await Task.Delay(1000);

                    if (client.IsConnected)
                    {
                        var s = new Document {
                            { "time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "oC", rand.Next(36, 40) },
                        };

                        client.Publish(topic, s.ToString());
                    }
                }
            });

            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "c": client.Connect(); break;
                    case "d": client.Disconnect(); break;
                }
            }
        }
    }
}
