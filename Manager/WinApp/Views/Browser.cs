using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinApp
{
    class Browser : System.Windows.Window
    {
        public Browser()
        {
            var ver = this.GetType().Assembly.GetName().Version;
            Title = $"{Config.Title} {Config.SubTitle} {ver}";

            System.Mvc.Engine.Register(typeof(Views.BaseView), result => {
                Dispatcher.InvokeAsync(() => {
                    var view = result.View as Views.BaseView;
                    var context = view.ViewContext;
                    var content = context.Result as UIElement;

                    if (content == null) return;

                    var layout = context.Layout as UIElement;
                    if (layout is Window)
                    {
                        var dlg = (Window)layout;
                        dlg.Content = content;
                        dlg.ShowDialog();

                        return;
                    }

                    if (layout == null)
                    {
                        layout = new Border { Child = content };
                    }
                    Content = layout;
                });
            });

            Closing += (s, e) => {
                App.End();

                //e.Cancel = true;
                //if (MessageBox.Show("Sure to exit?", "A.K.S", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                //    e.Cancel = false;
                //    App.End();
                //}
            };

        }
    }
}
