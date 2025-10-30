using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BsonData;
using System.Reflection;

namespace Vst.Server
{

    public class Topic
    {
        static public string CreateInternalTopic(string name)
        {
            return $"#ytruj_@x67xkd835q/{name.ToLower()}";
        }
    }

    public abstract class ServerBase
    {
        ControllerMap _controllers;
        protected ControllerMap Controllers
        {
            get
            {
                if (_controllers == null)
                {
                    _controllers = new ControllerMap(this.GetType().Assembly);
                }
                return _controllers;
            }
        }

        public ServerBase()
        {
            _controllers = new ControllerMap(this.GetType().Assembly);
        }

        #region Process
        /// <summary>
        /// Khởi tạo process information
        /// </summary>
        public virtual void InitProcessInfo()
        {
            var i = new ProcessInfo
            {
                FullPath = Process.GetCurrentProcess().MainModule.FileName
            };
            var name = i.ObjectId;

            var config = Config.Load(i.Path, name);
            i.Copy(config);

            ProcessInfo = i;
        }
        public ProcessInfo ProcessInfo { get; protected set; }
        #endregion

        #region Main Thread
        protected abstract void InitMainThread();
        protected abstract void StartConnection();
        protected virtual void InitDatabase() { }
        protected virtual MethodInfo GetInternalMethod(string action) => this.FindMethod(action);
        protected virtual void ProccessCommand(string cmd, string[] args) { }
        #endregion

        protected virtual void OnClosing()
        {

        }
        public void Stop()
        {
            OnClosing();
        }

        protected virtual void OnStarted()
        {
            WaitAsync(500, () => {
                Console.Title = ProcessInfo.Name;
            });
        }

        public void Wait(int delay, Action callback)
        {
            System.Threading.Thread.Sleep(delay);
            callback();
        }
        public void WaitAsync(int delay, Action callback) => Task.Run(() => Wait(delay, callback));

        protected virtual List<Action> GetStartingSteps()
        {
            return new List<Action> {
                InitProcessInfo,
                InitDatabase,
                InitMainThread,
                StartConnection,
            };
        }
        public void Start()
        {
            foreach (var a in GetStartingSteps())
            {
                a?.Invoke();
            }
            OnStarted();
        }

        public Clock SystemClock { get; private set; } = new Clock();

        public abstract void Publish(string topic, Document data);
        public abstract void SendInternalRequest(string server, string cid, string action, Document data);
    }
    public abstract class ServerBase<T> : ServerBase
        where T : ServerDatabase, new()
    {
        public ServerBase()
        {
            MainDB = new T();
        }
        #region Database
        public static T MainDB { get; protected set; }
        protected override void InitDatabase()
        {
            MainDB.Connect(ProcessInfo.Path);
        }
        #endregion

        protected override void OnClosing()
        {
            MainDB?.Disconnect();
            base.OnClosing();
        }
    }
}
