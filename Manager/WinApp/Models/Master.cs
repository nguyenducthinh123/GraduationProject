using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    using PI = Vst.Server.ProcessInfo;
    public class Master : PI
    {
        public bool IsReady { get; set; }
        public bool AutoReset { get; set; }
        public Manager Manager { get; set; }
        public Master(Manager manager)
        {
            Manager = manager;
            FullPath = $"{Environment.CurrentDirectory}\\MasterServer.exe";
        }

        public void Reset()
        {
            Stop();
            Start();
        }
        public override void Stop()
        {
            IsReady = false;
            base.Stop();
        }
        public override bool Start()
        {
            if (IsReady) return false;

            AutoReset = true;
            base.Start();

            while (!IsAlive) { };

            IsReady = true;
            return true;
        }
    }
}
