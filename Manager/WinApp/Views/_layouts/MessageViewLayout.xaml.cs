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
    /// Interaction logic for MessageViewLayout.xaml
    /// </summary>
    public partial class MessageViewLayout : UserControl
    {
        public MessageViewLayout()
        {
            InitializeComponent();
        }

        public void Add(Models.BrokerMessage message, bool top)
        {
            var item = new MessageItemView {
                DataContext = message
            };
            item.MouseUp += (s, e) => {
                MessageContentText.Text = message.Content;
            };
            if (top)
            {
                MainContent.Children.Insert(0, item);
            }
            else
            {
                MainContent.Children.Add(item);
            }
        }

    }
}
