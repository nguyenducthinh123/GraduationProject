using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Vst.Controls;

namespace WinApp.Views.Home
{
    class Index : BaseView<MessageViewLayout, Models.Manager>
    {
        protected override void RenderCore(ViewContext context)
        {
            context.Title = "Monitoring";
            foreach (var e in App.Manager.Messages)
            {
                MainView.Add(e, false);
            }

            Action<Models.BrokerMessage> ev = e => {
                DispatcherUpdate(() => MainView.Add(e, true));
            };
            Model.OnMessageCreated += ev;
            Disposing += () => Model.OnMessageCreated -= ev;

            base.RenderCore(context);
        }
    }
}
