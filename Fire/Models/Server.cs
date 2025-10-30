using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fire
{
    using VS = Vst.Server;

    class DeviceDB : VS.DeviceCollecton<Device>
    {
        public DeviceDB() : base(Server.MainDB)
        {
        }
    }
    class Server : VS.SlaveServer
    {

        DeviceDB _devices;
        public DeviceDB Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new DeviceDB();
                return _devices;
            }
        }

        public long[] ElectricalCost { get; private set; }

        protected override void OnStarted()
        {
            var conf = VS.Config.Load(MainDB.ConnectionString, "config");
            ElectricalCost = conf.GetArray<long>("e-cost").ToArray();

            base.OnStarted();

            int index = 0;
            SystemClock.OneSecond += async () => {
                var devices = Devices.SelectAll();
                if (devices.Count == 0)
                    return;

                if (index >= devices.Count)
                    index = 0;

                var device = (Device)devices[index];

                try
                {
                    var count = await device.Read();
                    Console.Write(Screen.Now($"{device.ObjectId} -> "));
                    if (count == 0)
                    {
                        Console.WriteLine();
                    }
                    else
                    {
                        Screen.Info($"{count} record(s)");
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(Screen.Now(""));
                    Screen.Error($"{device.ObjectId} -> {ex.Message}");
                }

                ++index;
            };
            SystemClock.StartAsync();
        }
    }
}
