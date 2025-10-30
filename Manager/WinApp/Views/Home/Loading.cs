using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp.Views.Home
{
    class Loading : BaseView<LoadingView, Models.ServersModel>
    {
        protected override void RenderCore(ViewContext context)
        {
            base.RenderCore(context);

            MainView.Progress.Maximum = Model.Count;
            Model.OnProcessStarted += (p) => {
                MainView.Dispatcher.InvokeAsync(() => {
                    MainView.Progress.Value++;
                });
            };
        }

        protected override void OnReady()
        {
            Task.Run(async () => {
                await Task.Delay(500);
                Model.Start(() => {
                    MainView.Dispatcher.Invoke(() => App.Request("home/ready"));
                });
            });
        }

    }
}
