using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;
using Vst.Server;

namespace Master
{
    public class Server : ServerBase<DB>
    {
        IMqttServer _broker;
        bool _publishing;
        public event Action<string, bool> ClientConnectionChanged;
        public event Action<object> MessageReceived;

        public string UserName { get; set; }
        public string Password { get; set; }

        bool ProcessMessage(string clientId, string topic, byte[] payload)
        {
            if (_publishing)
            {
                _publishing = false;
                return true;
            }

            try
            {
                if (topic == null) { return false; }

                if (topic[0] != '#')
                {
                    var req = new RequestContext(topic);
                    if (req.ControllerName != null)
                    {
                        bool serverFound = req.ServerName == "manager";
                        if (serverFound)
                        {
                            Console.Write(Screen.Now(""));
                            Screen.Warning(topic);
                        }
                        else
                        {
                            serverFound = MainDB.Slaves.FindOne(req.ServerName) != null;
                        }
                        if (serverFound)
                        {
                            var msg = new Payload();
                            msg.Add("cid", clientId);
                            msg.Add("url", topic);
                            msg.Add("value", payload);

                            topic = Topic.CreateInternalTopic(req.ServerName);
                            Publish(topic, msg.ToJSON());

                            return false;
                        }
                    }
                }
                else {
                }
            }
            catch (Exception e)
            {
                Screen.Error(e.Message);
            }
            return true;
        }
        protected override void InitMainThread()
        {
        }

        protected override void StartConnection()
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                 .WithConnectionBacklog(100)
                 .WithDefaultEndpointPort(1883)
                 .WithApplicationMessageInterceptor(context =>
                 {
                     bool accept = ProcessMessage(context.ClientId,
                         context.ApplicationMessage.Topic, context.ApplicationMessage.Payload);
                     MessageReceived?.Invoke(context);

                     context.AcceptPublish = accept;
                 })
                 .Build();

            _broker = new MqttFactory().CreateMqttServer();

            _broker.ClientConnected += _broker_ClientConnected;
            _broker.ClientDisconnected += _broker_ClientDisconnected;
            _broker.ClientSubscribedTopic += _broker_ClientSubscribedTopic;
            _broker.ClientUnsubscribedTopic += _broker_ClientUnsubscribedTopic;

            Task.Run(() => _broker.StartAsync(optionsBuilder));
        }

        private void _broker_ClientUnsubscribedTopic(object sender, MqttClientUnsubscribedTopicEventArgs e)
        {
            Screen.Info("[-UNSUBSCRIBE] " + e.TopicFilter);
        }
        private void _broker_ClientSubscribedTopic(object sender, MqttClientSubscribedTopicEventArgs e)
        {
            Screen.Info("[+SUBSCRIBE] " + e.TopicFilter.Topic);
        }
        private void _broker_ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Screen.Error(Screen.Now(e.ClientId));
            ClientConnectionChanged?.Invoke(e.ClientId, false);
        }
        private void _broker_ClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Screen.Success(Screen.Now(e.ClientId));
            ClientConnectionChanged?.Invoke(e.ClientId, true);
        }

        public void Publish(string topic, byte[] payload)
        {
            _publishing = true;
            var am = new MqttApplicationMessage
            {
                Topic = topic,
                Payload = payload,
                QualityOfServiceLevel = 0,
                Retain = false,
            };

            Task.Run(() => _broker.PublishAsync(am));
        }
        public override void Publish(string topic, Document data)
        {
            if (data != null)
            {
                Publish(topic, data.ToString().UTF8());
            }
        }
        public override void SendInternalRequest(string server, string cid, string action, Document data)
        {
        }

        static public string CreateSlavePath(string slavePath)
        {
            var v = Environment.CurrentDirectory.Split('\\');
            var s = slavePath.Split('\\');

            var l = new List<string>();
            var count = 0;
            while (l.Count < s.Length && l.Count < v.Length && v[count] == s[count])
                count++;

            for (int i = 0; i < v.Length - count; i++)
                l.Add("..");
                
            while (count < s.Length)
            {
                l.Add(s[count++]);
            }

            return string.Join("\\", l);
        }
        public override void InitProcessInfo()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            ProcessInfo = new ProcessInfo {
                FullPath = process.MainModule.FileName,
                Name = "SERVER MASTER",
                ThreadInterval = 1,
            };
        }
        //protected override void OnStarted()
        //{
        //    base.OnStarted();

        //    var lst = new DocumentList();
        //    MainDB.Slaves.ForEach(e => {
        //        e.Topic = new BsonData.ObjectId();

        //        MainDB.Slaves.Update(e);
        //        Config.Save(e.Path, e.ObjectId, e);
        //    });

        //    Console.Title = ProcessInfo.Name;
        //}
    }
}
