using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class ServerPath
    {
        string[] _items;
        public string Path { get; private set; }
        public ServerPath(string path)
        {
            Path = path;
        }
        public string this[int index]
        {
            get
            {
                if (_items == null)
                {
                    _items = Path.Split('/');
                }
                return index >= _items.Length ? null : _items[index];
            }
            set
            {
                if (index < _items.Length)
                    _items[index] = value;
            }
        }
        public override string ToString()
        {
            return string.Join("/", _items);
        }
        public void CorrectPath(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var s = this[i];
                _items[i] = s.ToCodeName();
            }
        }
    }
    public class RequestContext
    {
        public ServerPath Uri { get; set; }
        public string ServerName => Uri[0];
        public string ControllerName => Uri[1];
        public string ActionName => Uri[2];
        public string ClientId { get; set; }
        public string Body { get; set; }
        Document _payload;
        public Document Payload
        {
            get
            {
                if (_payload == null)
                {
                    _payload = Document.Parse(Body);
                }
                return _payload;
            }
            set => _payload = value;
        }
        public MemoryPacket MemoryPacket { get; set; }

        public RequestContext() { }
        public RequestContext(MemoryPacket packet) 
            : this(packet.Topic)
        {
            MemoryPacket = packet;
        }
        public RequestContext(string url)
        {
            Uri = new ServerPath(url);
            ClientId = Uri[3];
        }
    }
}
