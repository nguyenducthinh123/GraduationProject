using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTGO;
using Vst.Server;

namespace Alarm
{
    using VS = Vst.Server;
    class Server : VS.LogServer
    {
        protected override VS.LogProcessor CreateLogProcessor() => new LogProcessor();
        Channels _channels;

        protected override void ConnectToBroker()
        {
            _channels = new Channels();
            _channels.Detect();

            if (_channels.Count == 0)
                _channels.Add(new Sim("DEMO"));

            base.ConnectToBroker();
        }
        protected override void OnStarted()
        {
            base.OnStarted();
            SystemClock.OneSecond += () => {
                foreach (var s in _channels)
                    s.Execute();
            };
        }
        protected override void ProcessInternalRequest(string action, RequestContext context)
        {
            if (action[0] == '#')
            {
                if (_channels.Count > 0)
                {
                    Sim sim = null;
                    foreach (var e in _channels)
                    {
                        if (sim == null || e.Commands.Count < sim.Commands.Count)
                        {
                            sim = e;
                        }
                    }

                    var number = context.Payload.GetString("number");
                    if (action[1] == 's')
                    {
                        sim.CreateSMS(number, context.Payload.Message);
                    }
                    else
                    {
                        sim.CreateCALL(number);
                    }
                }
                return;
            }

            if (action == "log")
            {
                var id = context.Payload.ObjectId;
                var log = context.Payload.ValueContext.ChangeType<VS.Log>();
                var time = log.Time;

                return;
            }
            base.ProcessInternalRequest(action, context);
        }
    }
}
