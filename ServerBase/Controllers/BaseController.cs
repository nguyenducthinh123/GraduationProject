using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public abstract class Controller
    {
        public ServerBase Processor { get; set; }
        public RequestContext RequestContext { get; set; }
        public Document Payload => RequestContext.Payload;
        Document _value;
        public Document ValueContext
        {
            get
            {
                if (_value == null)
                    _value = Payload.ValueContext;
                return _value;
            }
        }
        public Document Success() => new Document();
        public Document Success(object value) => new Document { Value = value };
        public Document Error(int code) => new Document { Code = code };
        public Document Error(int code, string message) => new Document { Code = code, Message = message };

        public void Publish(string topic, Document data) => Processor.Publish(topic, data);
        public void Publish(string topic, string name, object data) => Processor.Publish(topic, new Document { { name, data } });

        public void CheckManager(Action callback)
        {
            if (RequestContext.ServerName == "manager")
            {
                callback();
            }
        }
    }
}
