using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace WinApp.Controllers
{
    //class TestController : BaseController
    //{
    //    static Random _demo = new Random();
    //    public MqttClient Client => Master.MqttClient;
    //    public override object Index()
    //    {
    //        //DB.ServerList.Broker.Hidden = false;
    //        return View(Client);
    //    }
    //    public object Connect()
    //    {
    //        //Client.Connect();
    //        return null;
    //    }
    //    public object Disconnect()
    //    {
    //        //Client.Disconnect();
    //        return null;
    //    }

    //    protected object TestView(string topic, object value)
    //    {
    //        Client.TestPacket = new Document {
    //            Url = topic,
    //            Value = value
    //        };
    //        return View(Client);
    //    }
    //    protected object TestAccountView(string topic, object value)
    //    {
    //        return TestView(topic, new Document {
    //            Token = Client.Token,
    //            Value = value
    //        });
    //    }

    //    public object Account(string id)
    //    {
    //        switch (id)
    //        {
    //            case "login":
    //                return TestView("account/personal/login", 
    //                    new Document { UserName = "admin", Password = "1234" });

    //            case "logout":
    //                return TestAccountView("account/personal/logout", null);

    //            case "add":
    //                return TestAccountView("account/manager/create", 
    //                    new Document { UserName = "0902186628", Role = "Customer" });

    //            case "device":
    //                return TestAccountView("account/device/add", 
    //                    new Document { ObjectId = "0001", Model = "FIRE", Version = "1.0" });

    //        }
    //        return View();
    //    }
    //    public object Device(string id)
    //    {
    //        switch (id)
    //        {
    //            case "status":
    //                var v = new { U = _demo.Next(215, 230), I = _demo.Next(20, 200) };
    //                return TestView("firmware/upload/status/0001", 
    //                    new Document { Value = v });

    //            case "connect":
    //                return TestView("firmware/upload/connect/0001", 
    //                    new Document { Model = "FIRE", Version = "1.0" });

    //            case "list":
    //                return TestView("fire/device/list", new Document { { "items", "['0000000006']" } });

    //        }
    //        return View();
    //    }

    //    public object Log(string id)
    //    {
    //        var topic = $"firmware/user/log";
    //        var doc = new Document { 
    //            ObjectId = "0001", 
    //            Action = id,
    //            BD = DateTime.Now,
    //        };
    //        return TestView(topic, doc);
    //    }
    //}
}
