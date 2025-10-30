using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PI = Vst.Server.ProcessInfo;

namespace WinApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public void RedirectToAction(string action, params object[] args)
        {
            var context = System.Mvc.Engine.RequestContext;
            Request($"{context.ControllerName}/{action}", args);
        }
        static public void Request(string url, params object[] args) => 
            System.Mvc.Engine.Execute(url ?? GetCurrentUrl(), args);
        static public void GoHome() => Request("home");

        static public void Post(Document context) => System.Mvc.Engine.Execute(GetCurrentUrl(), context);
        static public string GetCurrentUrl()
        {
            var context = System.Mvc.Engine.RequestContext;
            return $"{context.ControllerName}/{context.ActionName}";
        }
        static public void Start()
        {
            Config.Load(Environment.CurrentDirectory + "/app_data/");

            var wnd = new Browser();
            wnd.Show();

            Request(Config.StartUrl ?? "home");
        }
        static public void End()
        {
            System.Mvc.Engine.Exit();
            Manager.Stop();
        }
        static public AppUser User { get; set; } = new AppUser { Role = "Admin" };
        static public Models.Manager Manager => Models.ServersModel.Manager;
        protected override void OnStartup(StartupEventArgs arg) => Start();
    }
}