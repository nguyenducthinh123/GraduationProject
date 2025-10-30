using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace History
{
    using BsonData;
    class Database : Vst.Server.ServerDatabase
    {
        Collection _today;
        public Collection Today
        {
            get
            {
                if (_today == null)
                    _today = GetCollection(nameof(Today));
                return _today;
            }
        }

        Collection _devices;
        public Collection Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = GetCollection(nameof(Devices));
                }
                return _devices;
            }
        }

        Collection _everyDay;
        public Collection EveryDay
        {
            get
            {
                if (_everyDay == null)
                    _everyDay = GetCollection(nameof(EveryDay));
                return _everyDay;
            }
        }
    }
}
