using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vst.Controls;

namespace WinApp.Views.Server
{
    using TC = TableColumn;
    using TE = EditorInfo;
    class Index : BaseView<DataListViewLayout, Models.ServersModel>
    {
        protected override void RenderCore(ViewContext context)
        {
            base.RenderCore(context);
            context.Title = "SERVERS";
            context.TableColumns = new TC[] {
                new TC { Name = "name", Caption = "Name", Width = 150 },
                //new TC { Name = "MemorySize", Width = 100 },
                //new TC { Name = "ThreadInterval", Width = 100 },
                new TC { Name = "IsAlive", Width = 120 },
            };

            MainView.OpenItemContext.Url = "open";
            MainView.Header.CreateAction(new ActionContext("Start", () => App.Request("home/loading", Model)));
            MainView.Header.CreateAction(new ActionContext("Stop", () => {
                var res = MessageBox.Show("Are you sure to stop all of servers", "A.K.S", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Model.Stop();
                }
            }));
        }
    }

    class Edit : EditView
    {
        protected override void RaiseUpdate()
        {
            ViewContext.Update(null);
            base.RaiseUpdate();
        }
        protected override void RenderCore(ViewContext context)
        {
            base.RenderCore(context);

            var p = (Document)context.Model;

            context.Title = p.ObjectId.ToUpper();
            context.Editors = new TE[] {
                new TE { Name = "name", Caption = "Server Name" },
                new TE { Name = "path", Caption = "Execution Path" },
                new TE { Name = "fields", Caption = "Device Information Fields", Required = false },
                //new TE { Name = "MemorySize", Caption = "Memory Size (MB)", Type = "number" },
                //new TE { Name = "ThreadInterval", Caption = "Threading Interval (ms)", Type = "number" },
            };

            context.Binding();
        }
    }
    class Add : BaseView
    {
        protected override object CreateLayout() => null;
        protected override object CreateView()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { 
                Filter = "Execution File|*.exe|All Files|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                App.RedirectToAction("add", dlg.FileName);
            }
            return null;
        }
        public override void Render(object model)
        {
        }
    }
}
