using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire
{
    class Log : Vst.Server.Log
    {
        static public long GetLevel(Vst.Server.Log log) => log.GetValue<long>("s");
        static public string GetMessage(long level)
        {
            switch (level)
            {
                case 1: return "qua tai";
                case 2: return "chap dien";
                case 3: return "chay";
            }
            return null;
        }
    }
}
