using BsonData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    using PI = Vst.Server.ProcessInfo;
    class ManagerInfo : PI
    {
        Manager server;
        public override bool IsAlive => true;
        public ManagerInfo(Manager manager)
        {
            ObjectId = "manager";
            Name = "Manager";
            server = manager;
        }
        public override bool Start()
        {
            server.Start();
            return true;
        }
        public override void Stop()
        {
            server.SystemClock.Stop();
        }
    }

    public class Manager : Vst.Server.SlaveServer<ServerDB>
    {
        public Manager()
        {
            MainDB = new ServerDB();
            MainDB.Connect(Environment.CurrentDirectory);

            Master = new Master(this);
            ProcessInfo = new ManagerInfo(this);

            //foreach (var e in DevicesCollection.SelectAll())
            //{
            //    e.Remove("cost");
            //    DevicesCollection.Update(e);
            //}
        }

        #region SERVERS
        public Master Master { get; private set; }
        #endregion

        public void CheckSim()
        {
            Sim.Channels.Detect(lst =>
            {
                if (lst.Count == 0)
                {
                    CreateMessage("warning", "Sim card not found");
                }
                else
                {
                    foreach (var s in lst)
                    {
                        CreateMessage("success", $"Sim card ready in {s.PortName}");
                    }
                }
            });

            Sim.DefaultChannel.OnSending += (s, c) =>
            {
                CreateMessage(null, $"{s.PortName}: {c.Name} to {c.Number}");
            };
            SystemClock.OneSecond += Sim.DefaultChannel.Execute;
        }

        public override void InitProcessInfo()
        {
        }
        protected override void RaiseConnected()
        {
            CreateMessage("success", "Broker Connected");
            base.RaiseConnected();
        }
        protected override void RaiseConnectionLost()
        {
            CreateMessage("danger", "Broker Lost");
        }
        protected override void OnStarted()
        {
            CheckSim();

            SystemClock.OneTick += () =>
            {
                if (Master.AutoReset && !Master.IsAlive)
                {
                    Master.Reset();
                }
            };
            SystemClock.StartAsync();
        }

        public void SendDeviceInfo(string server, DocumentList items)
        {
            SendInternalRequest(server, null, "device/update", new Document { Items = items });
        }

        protected override void ProcessRequest(Vst.Server.RequestContext context)
        {
            base.ProcessRequest(context);
        }
        protected override void ProcessReceivedData(string topic, Document context)
        {
            base.ProcessReceivedData(topic, context);
        }

        #region COLLECTIONS
        public Slaves SlavesCollection => MainDB.Slaves;
        public Accounts AccountCollection => MainDB.Accounts;
        public Devices DevicesCollection => MainDB.Devices;
        #endregion

        #region MESSAGE
        public event Action<BrokerMessage> OnMessageCreated;
        public BrokerMessage CreateMessage(string type, string message)
        {
            var one = new BrokerMessage(type, message);
            Messages.Push(one);
            OnMessageCreated?.Invoke(one);

            return one;
        }
        public BrokerMessage CreateMessage(string message) => CreateMessage(null, message);
        public Stack<BrokerMessage> Messages { get; private set; } = new Stack<BrokerMessage>();
        #endregion

    }
}
