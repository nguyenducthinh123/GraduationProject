using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinApp.Views
{
    /// <summary>
    /// Interaction logic for TestMqttLayout.xaml
    /// </summary>
    public partial class TestMqttLayout : UserControl
    {
        //public void ShowActions()
        //{
        //    Dispatcher.InvokeAsync(() => {
        //        ButtonSend.IsVisible = Client.IsConnected;
        //    });
        //}

        //Models.MqttClient Client => (Models.MqttClient)DataContext;

        //public TestMqttLayout()
        //{
        //    InitializeComponent();

        //    DataContextChanged += (s, e) => {
        //        Client.ConnectionChanged += ShowActions;
        //        Client.Responsed += (url, doc) => {
        //            Dispatcher.InvokeAsync(() => {
        //                ResponseContext.Text = doc.ToString();
        //            });
        //        };
        //    };

        //    ButtonSend.Click += (s, e) => {
        //        var topic = Topic.Text;
        //        if (string.IsNullOrWhiteSpace(topic))
        //            return;
        //        var payload = Payload.Text;
        //        var packet = new Vst.Server.MemoryPacket(topic, payload.UTF8());

        //        Client.Publish(topic, payload);
        //    };

        //    ShowActions();
        //}
    }
}
