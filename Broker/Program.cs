using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Broker
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new MqttServer();
            server.Start();
        }
    }
}
