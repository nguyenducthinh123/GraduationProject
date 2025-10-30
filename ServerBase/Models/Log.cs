using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst.Server
{
    public class Log : Document
    {
        new public object Status
        {
            set => Push("s", value);
            get
            {
                TryGetValue("s", out object v);
                return v;
            }
        }
        public DateTime? Time
        {
            get => GetDateTime("t");
            set => Push("t", value?.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        static public Action<string, Document> SaveLog { get; set; }
        static public Action<string, string, Document, bool> SendAlarm { get; set; }
    }
    public class TodayLog
    {
        public Log Current { get; set; }

        DocumentList _items;
        public Document Add(long level, DateTime? time)
        {
            Current = new Log {
                Status = level,
                Time = time,
            };
            Add(Current);
            return Current;
        }
        public void Add(Document log)
        {
            if (_items.Count == 10)
                _items.RemoveAt(_items.Count - 1);

            _items.Insert(0, log);
        }
        public TodayLog(Device device)
        {
            _items = device.GetDocumentList("logs");
            if (_items == null)
            {
                _items = new DocumentList();
            }
            device.Push("logs", _items);
        }
    }

}
