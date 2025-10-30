using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace History
{
    class Summary : Document
    {
        public void Add(DateTime? date)
        {
            SelectContext($"{date:yyMM}", doc => {
                var key = date.Value.Day.ToString();
                var v = doc.GetValue<int>(key);
                doc.Push(key, v + 1);
            });
        }
    }
    class Device : Vst.Server.Device
    {
        public void GetLogs(Action<Document> callback)
        {
            SelectContext("logs", callback);
        }
        public void GetSummary(Action<Summary> callback)
        {
            SelectContext("sum", callback);
        }
    }
}
