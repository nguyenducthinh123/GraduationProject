using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire
{
    public class ReceiveRecord : Document
    {
        public T GetNumber<T>(string name)
        {
            T res = default;
            Find(name, o => {
                res = (T)Convert.ChangeType(o, typeof(T));
            });
            return res;
        }

        public double Voltage => GetNumber<long>("Voltage");

        double _i = -1;
        public double Current
        {
            get
            {
                if (_i < 0)
                    _i = GetNumber<double>("Current");
                return _i;
            }
        }

        double _ip = -1;
        public double Ipeak
        {
            get
            {
                if (_ip < 0)
                    _ip = GetNumber<double>("Ipeakmax");

                return _ip == 0 ? Ipeak10 : _ip;
            }
        }
        public double Upeak => GetNumber<long>("Upeakmax");
        public double Ileak => GetNumber<long>("Ileak");
        public double Ileak10 => GetNumber<long>("Ileakmax10");

        double _ip10 = -1;
        public double Ipeak10
        {
            get
            {
                if (_ip10 < 0) _ip10 = GetNumber<double>("Ipeakmax10s");
                return _ip10;
            }
        }
        public double Upeak10 => GetNumber<long>("Upeakmax10s");

        double _p = -1;
        public double TotalPower
        {
            get
            {
                if (_p < 0)
                    _p = GetNumber<long>("Power");
                return _p;
            }
        }
        public double Cos => GetNumber<double>("Cosphi");
        public long Relay { get => GetNumber<long>("Relay"); set => Push("Relay", value); }
        public long Fire => GetNumber<long>("Fire");
        public virtual int Classifier(double warning, double danger)
        {
            Func<int, int> set = s => { 
                Add("s", s);
                return s; 
            };
            if (Fire > 0)
            {
                return set(3);
            }
            if (Ipeak10 >= danger)
            {
                return set(2);
            }

            if (Current >= warning)
            {
                return set(1);
            }

            return 0;
        }

        DateTime? _time;
        public DateTime? Time
        {
            get
            {
                if (_time == null)
                    _time = GetDateTime("received_time");
                return _time;
            }
            set => _time = value;
        }

        #region DEMO
        public static ReceiveRecord CreateDemo(double i, double ipeak, long fire, DateTime? time)
        {
            if (ipeak == 0)
                ipeak = i;

            var r = new ReceiveRecord { 
                _time = time ?? DateTime.Now,
                _i = i,
                _ip = ipeak,
                _ip10 = ipeak,
            };
            if (fire != 0) r.Push("Fire", fire);
            return r;
        }
        #endregion
    }
    public class ElectricalRecord : Document
    {
        public double TotalPower { get; private set; }
        bool ready;
        public ElectricalRecord(ReceiveRecord r)
        {
            Add("t", r.Time);
            Add("i", r.Current);
            Add("u", r.Voltage);
            Add("p", TotalPower = r.TotalPower);
            Add("f", r.Cos);
            Add("l", r.Ileak10);

            var relay = r.Relay;
            if (relay != 0)
            {
                Add("r", relay);
            }
        }
        public ElectricalRecord Calculate(ReceiveRecord first, double lastPower, long[] cost)
        {
            if (!ready)
            {
                if (lastPower != TotalPower)
                {
                    int i = 0;
                    while (i < cost.Length)
                    {
                        if (cost[i] > TotalPower)
                            break;

                        i += 2;
                    }
                    Add("m", TotalPower * (i == 0 ? 0 : cost[i - 1]) / 1000);
                    Add("d", TotalPower - first.TotalPower);
                }
                ready = true;
            }

            return this;
        }
    }
    public class RecordQueue : LinkedList<ElectricalRecord>
    {
        public ReceiveRecord DayRecord { get; set; }
        public ReceiveRecord LastRecord
        {
            get => _lastRecord;
            set
            {
                if (_lastRecord != value)
                {
                    _lastRecord = value;
                    while (Count >= 10)
                    {
                        RemoveFirst();
                    }
                    var e = new ElectricalRecord(value);
                    AddLast(e);
                }
            }
        }
        ReceiveRecord _lastRecord;

        public RecordQueue Calculate(long[] cost)
        {
            double p = 0;
            foreach (var r in this)
            {
                r.Calculate(DayRecord, p, cost);
                p = r.TotalPower;
            }
            return this;
        }
    }
}
