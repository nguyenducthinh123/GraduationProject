using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire
{
    using Vst.Server;
    public class TimeLine
    {
        public int MinuteDuration { get; private set; }
        public DateTime Time { get; private set; }
        public TimeLine(int duration)
        {
            MinuteDuration = duration;
            Time = DateTime.Now;
        }
        public void CheckTime(Action callback)
        {
            if (DateTime.Now >= Time)
            {
                callback();
                Time = Time.AddMinutes(MinuteDuration);
            }
        }
    }

    class Device : Vst.Server.Device
    {
        long lay_nguong(string name, long default_value)
        {
            var v = GetValue<long>(name);
            if (v == 0) v = default_value;

            return (v << 1) + (v << 3);
        }
        public long IWarning => lay_nguong("iWarning", 40);
        public long IDanger => lay_nguong("iDanger", 100);

        long _relay;
        public long Relay
        {
            get => _relay;
            set
            {
                if (_relay != value)
                {
                    _relay = value;
                    Processor.Publish($"alarm/{ObjectId}", new Document {
                        { "r", _relay }
                    });
                }
            }
        }
        RecordQueue _records = new RecordQueue();
        public RecordQueue GetRecords()
        {
            return _records.Calculate(Processor.ElectricalCost);
        }

        static ElectricalReader _reader = new ElectricalReader();
        public ElectricalReader Reader => _reader;

        public static Server Processor { get; set; }
        public static DeviceDB DB => Processor.Devices;
        public static Device FindOne(string id) => DB.FindOne(id);

        TimeLine[] time_line = new TimeLine[] {
            null,
            new TimeLine(15),
            new TimeLine(1),
            new TimeLine(1),
        };

        public void Read(DocumentList lst, bool demo = false)
        {
            long danger = 0;
            ReceiveRecord log = null;

            bool test = ObjectId == "0000000006";
            var iDanger = IDanger;
            var iWarning = IWarning / 10;

            foreach (ReceiveRecord r in lst)
            {
                if (_records.LastRecord?.Time == r.Time)
                    continue;

                var d = r.Classifier(iWarning, iDanger);

                if (d > danger)
                {
                    log = r;
                    danger = d;
                }
                else
                {
                    this.Relay = r.Relay;
                }

                _records.LastRecord = r;
            }

            if (danger != 0)
            {
                SaveLog(danger, log);
            }
            else
            {
                Logs.Current = null;
            }
        }
        public async Task<int> Read()
        {

            if (_records.DayRecord == null)
            {
                LoadDayRecord();
            }
            var end = DateTime.Now;
            var sta = end;

            if (_records.LastRecord != null)
                sta = _records.LastRecord.Time.Value.AddSeconds(1);

            var lst = await _reader.GetHistory(ObjectId, sta, end);
            if (lst.Count > 0)
                Read(lst.ChangeType<ReceiveRecord>());

            return lst.Count;
        }
        public void LoadDayRecord()
        {
            var sta = DateTime.Today;
            Task.Run(async () => {
                var lst = await _reader.GetHistory(ObjectId, sta, sta);
                var r = new ReceiveRecord { Time = sta };
                if (lst.Count > 0)
                {
                    var f = lst[0].ChangeType<ReceiveRecord>();

                    r.Add("Power", f.TotalPower);
                }

                _records.DayRecord = r;

                Screen.Warning(ObjectId);
                Screen.Info(r.ToString());
            });
        }
        public async Task<Document> OnOff(Document data)
        {
            return await _reader.OnOff(ObjectId, data.Action);
        }
        public bool SaveLog(long level, ReceiveRecord r)
        {
            bool res = false;
            var old = Logs.Current == null ? 0 : Log.GetLevel(Logs.Current);

            if (old < level)
            {
                var time = r.Time;
                var log = Logs.Add(level, time);
                var id = ObjectId;

                if (log != null)
                {
                    Screen.Warning(Screen.Now($"Log level {level} sent to {id}"));

                    res = true;

                    Log.SaveLog(id, log);
                    var t = time_line[level];
                    t?.CheckTime(() => {
                        Log.SendAlarm(ObjectId, Log.GetMessage(level), Logs.Current, level >= 2);
                    });
                }
            }

            return res;
        }
        public void Simulate(string type)
        {
            var lst = new DocumentList();
            var t = DateTime.Now;

            if (type == "d")
            {
                var current = IDanger;
                for (int i = 0; i < 10; i++)
                {
                    lst.Add(ReceiveRecord.CreateDemo(current, 0, 0, t));
                    t.AddSeconds(1);
                }
            }
            else if (type == "f")
            {
                lst.Add(ReceiveRecord.CreateDemo(1, 0, 3, null));
            }
            else if (type == "w")
            {
                var current = IWarning / 10 + 1;

                for (int i = 0; i < 10; i++)
                {
                    lst.Add(ReceiveRecord.CreateDemo(current, 0, 0, t));
                    t.AddSeconds(1);
                }
            }

            Read(lst, true);
        }
    }
}
