using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    
    class ServersModel : Queue<Vst.Server.ProcessInfo>
    {
        public event Action<Vst.Server.ProcessInfo> OnProcessStarted;

        static public Manager Manager { get; private set; } = new Manager();
        public ServersModel()
        {
            Enqueue(Manager.Master);
            Enqueue(Manager.ProcessInfo);

            Manager.SlavesCollection.ForEach(p => Enqueue(p));
        }

        public void Stop()
        {
            Manager.Master.AutoReset = false;
            foreach (var p in this)
            {
                p.Stop();
            }
        }
        public void Start(Action completed)
        {
            foreach (var p in this)
            {
                p.Start();
                while (!p.IsAlive) { }

                OnProcessStarted?.Invoke(p);
                System.Threading.Thread.Sleep(100);
            }

            completed?.Invoke();
        }
    }
}
