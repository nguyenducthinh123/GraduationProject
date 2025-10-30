using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vst.Server
{
    public abstract class SlaveServer : SlaveServer<ServerDatabase> { }
    public abstract class SlaveServer<T> : ServerBase<T>
        where T: ServerDatabase, new()
    {

        #region MQTT
        //const string host = "broker.emqx.io";
        //const string host = "system.aks.vn";
        const string host = "localhost";

        static protected MQTT.Client _client;
        public MQTT.Client Client => _client;

        protected virtual string GetDefaultTopic() => Topic.CreateInternalTopic(ProcessInfo.ObjectId);
        
        /// <summary>
        /// Khởi tạo MqttClient và kết nối Broker
        /// </summary>
        /// <param name="interval"></param>
        protected override void InitMainThread()
        {
            _client = new MQTT.Client(host);
            _client.ID = $"##### {ProcessInfo.ObjectId.ToUpper()} #####";
            _client.SetCheckConnectionInterval(3);

            _client.Connected += RaiseConnected;
            _client.ConnectionLost += RaiseConnectionLost;
            _client.DataReceived += (topic, payload) => {

                Document doc = null;
                string message = null;
                try
                {
                    message = payload.UTF8();
                    doc = Document.Parse(message);
                    topic = doc.Url ?? topic;
                }
                catch
                {
                    Screen.Warning(topic);
                    Screen.Error("Payload invalid");
                    Screen.WriteLine(message ?? "null");
                }
                    
                try
                {
                    ProcessReceivedData(topic, doc);
                }
                catch
                {
                    Screen.Error("Something wrong on processing received data");
                }

            };
        }

        protected virtual void RaiseConnected()
        {
            if (host == "localhost")
            {
                Screen.Success(Screen.Now("Broker connected"));
                _client.Subscribe(GetDefaultTopic());
            }
        }
        protected virtual void RaiseDisconnected()
        {
        }
        protected virtual void RaiseReconnectFail()
        {
        }
        protected virtual void RaiseConnectionLost()
        {
        }
        protected override void StartConnection()
        {
            _client.Connect();
        }
        #endregion

        #region Processing
        protected virtual void ProcessReceivedData(string topic, Document context)
        {

            var request = new RequestContext
            {
                ClientId = context.ClientID,
                Payload = context.ValueContext,
                Uri = new ServerPath(topic),
            };
            var action = context.Action;
            if (action != null)
            {
                ProcessInternalRequest(action, request);
            }
            else
            {
                ProcessRequest(request);
            }
        }

        protected virtual void ProcessInternalRequest(string action, RequestContext context)
        {
            var method = GetInternalMethod(action);
            if (method == null)
            {
                Screen.Error("Internal method not found");
                return;
            }
            method.Invoke(this, new object[] { context });
        }
        protected virtual void ProcessRequest(RequestContext context)
        {
            context.Uri.CorrectPath(3);
            var mc = Controllers.FindMethods(context.ControllerName);
            if (mc == null) return;

            var md = mc.GetMethod(context.ActionName);
            if (md == null) return;

            var c = (Controller)Activator.CreateInstance(mc.Type);
            c.RequestContext = context;
            c.Processor = this;

            var res = md.Invoke(c, new object[] { }) as Document;
            if (res != null)
            {
                ProcessResponse(context, res);
            }
        }
        protected virtual void ProcessResponse(RequestContext context, Document result)
        {
            var topic = $"response/{context.ClientId}";
            if (result.Url == null)
            {
                result.Url = $"{context.ControllerName}/{context.ActionName}";
            }
            _client.Publish(topic, result.ToString());
        }
        #endregion

        #region SEND DATA
        public void Publish(string topic, string message) => _client.Publish(topic, message);
        public override void Publish(string topic, Document data) => _client.Publish(topic, data?.ToString() ?? "{}");
        public override void SendInternalRequest(string server, string cid, string action, Document dara)
        {
            var doc = new Document {
                ClientID = cid,
                Url = $"{ProcessInfo.ObjectId}/{action}",
                Value = dara,
            };
            Publish(Topic.CreateInternalTopic(server), doc);
        }
        #endregion

        public SlaveServer()
        {
            Log.SaveLog = (id, log) => {
                SendInternalRequest("history", id, "log/now", log);
            };
            Log.SendAlarm = (id, message, info, danger) => {
                Publish($"alarm/{id}", info);
                if (danger)
                {
                    SendInternalRequest("manager", id, "device/alarm", new Document {
                        Message = message,
                    });
                }
            };
        }
    }
}
