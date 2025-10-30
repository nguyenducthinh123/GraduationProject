using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VS = Vst.Server;

namespace Alarm
{
    class LogProcessor : VS.LogProcessor
    {
        protected override Document CalculateSummary(VS.TimeFolder folder)
        {
            long p = 0;
            var lst = folder.GetDocuments();
            foreach (var e in lst)
            {
                var s = e.GetDocument("s");
                if (s != null)
                {
                    var u = s.GetValue<long>("U");
                    var i = s.GetValue<long>("I");
                    p += u * i;
                }
            }
            return new Document {
                { "P", (double)p / lst.Count / 10 }
            };
        }
        protected override DocumentList GetLogCore(string action, DateTime? start, DateTime? end)
        {
            if (action == "detail")
            {
                return GetDetails(start, end);
            }
            if (action == "summary")
            {
                return GetSummary(start, end);
            }

            return null;
        }
    }
}
