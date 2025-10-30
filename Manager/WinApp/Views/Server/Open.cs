using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WinApp.Views.Server
{
    class Open : BaseView<OpenServerLayout>
    {
        protected override void RenderCore(ViewContext context)
        {
            var p = (Vst.Server.ProcessInfo)context.Model;
            context.Title = p.Name;

            base.RenderCore(context);
            

            bool isAlive = p.IsAlive;

            Action set_actions = () => {
                MainView.ButtonStop.Background = isAlive ? Brushes.Red : Brushes.DarkGreen;
                MainView.ButtonStop.Text = isAlive ? "Stop" : "Start";
                
                MainView.ButtonShow.Text = (p.Hidden ? "Show" : "Hide") + " Console";
            };

            set_actions();

            MainView.ButtonEdit.Click += (s, e) => App.RedirectToAction("edit", p);
            MainView.ButtonShow.Click += (s, e) => { 
                p.Hidden ^= true; 
                set_actions(); 
            };
            MainView.ButtonStop.Click += (s, e) => { 
                if (isAlive)
                {
                    p.Stop();
                }
                else
                {
                    p.Start();
                }
                set_actions();
            };
            MainView.ButtonDelete.Click += (s, e) => {
                var ask = $"        Delete the server {p.Name}        ";
                if (MessageBox.Show(ask, "A.K.S", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    App.Request("server/delete", p);
                }
            };
            MainView.ButtonPath.Click += (s, e) => {
                var dlg = new Microsoft.Win32.OpenFileDialog {
                    Filter = "Execution File|*.exe|All Files|*.*",
                };
                if (dlg.ShowDialog() == true)
                {
                    App.Request("server/setpath", dlg.FileName);
                }
            };
        }
    }
}
