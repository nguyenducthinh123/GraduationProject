using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Vst.Controls;

namespace WinApp.Views.Device
{
    class DeviceLayout : Grid
    {
        public DeviceLayout()
        {
            var header = new PageHeader { 
                Title = "Device",
            };
            var content = new TextBlock { 
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                TextWrapping = System.Windows.TextWrapping.Wrap,
            };
            Children.Add(header);
            Children.Add(new ScrollViewer { Content = content }.SetGridLayout(1, 0));

            RowDefinitions.Add(new RowDefinition { Height = new System.Windows.GridLength(50) });
            RowDefinitions.Add(new RowDefinition());

            header.SearchBox.Text = "0000000006";
            header.SearchBox.Changed += (s) => App.Request("device/index", s);
            header.CreateAction(new ActionContext { Text = "Copy", Value = System.Windows.Media.Brushes.Orange, Invoke = () => {
                System.Windows.Clipboard.SetText(content.Text);
            } });
            DataContextChanged += (s, e) => { 
                if (DataContext is ViewContext v)
                {
                    var d = (Models.Device)v.Model;
                    if (d != null)
                    {
                        header.SearchBox.Text = d.ObjectId;
                        content.Text = d.ToString();
                    }
                }
            };
        }
    }
    class Index : BaseView<DeviceLayout, Models.Device>
    {
    }
}
