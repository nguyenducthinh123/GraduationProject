using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApp
{
    public class App
    {
        static Vst.MQTT.Client _mqtt;
        static public Vst.MQTT.Client MQTT 
        {
            get
            {
                if (_mqtt == null)
                {
                    _mqtt = new Vst.MQTT.Client("broker.emqx.io");
                    _mqtt.Connected += () => {
                        _mqtt.Subscribe("device/status/0000000003");
                    };

                    _mqtt.DataReceived += (topic, payload) => {
                        int i = topic.LastIndexOf('/');
                        var k = topic.Substring(i + 1);

                        if (StatusMap.TryGetValue(k, out var lst) == false)
                        {
                            lst = new Queue<string>();
                            StatusMap.Add(k, lst);
                        }

                        var msg = System.Text.Encoding.ASCII.GetString(payload);

                        if (lst.Count == 10)
                            lst.Dequeue();

                        lst.Enqueue(msg);
                    };
                }
                return _mqtt;
            }
        }

        public static Dictionary<string, Queue<string>> StatusMap { get; private set; } = new Dictionary<string, Queue<string>>();
    }
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            App.MQTT.Connect();
        }
    }
}
