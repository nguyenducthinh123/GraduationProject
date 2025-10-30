using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

namespace System.Mvc
{
    public class Engine
    {
        public static ControllerCollection Controllers { get; private set; }
        public static ViewCollection Views { get; private set; }

        public static RequestContext RequestContext { get; private set; }
        public static void Register(Type baseViewType, Action<ActionResult> viewValidateCallback)
        {
            Controllers = new ControllerCollection();
            Views = new ViewCollection(baseViewType);

            ValidateActionResult = viewValidateCallback;
        }

        public static void Execute(string url, params object[] values)
        {
            var request = new RequestContext(url);
            foreach (var v in values)
                request.Values.Add(v);
            Execute(request);
        }
        public static void Execute(RequestContext request)
        {
            RequestContext = request;

            var controller = Controllers.CreateInstance(request);
            if (controller == null)
            {
                RequestContext.ErrorCode = "CONTROLLER";
            }
            else
            {
                controller?.Execute(request, null);
            }
        }
        public static Action<ActionResult> ValidateActionResult;

        static Stack<Thread> _threads;
        public static Thread BeginInvoke(Action action)
        {
            if (_threads == null)
            {
                _threads = new Stack<Thread>();
            }
            while (_threads.Count > 0)
            {
                if (_threads.Peek().IsAlive) break;
                _threads.Pop();
            }
            var th = new Thread(new ThreadStart(action));
            _threads.Push(th);

            th.Start();
            return th;
        }

        public static void Exit()
        {
            if (_threads != null)
            {
                while (_threads.Count > 0)
                {
                    var th = _threads.Pop();
                    if (th.IsAlive)
                        th.Abort();
                }
            }
        }
    }
}
