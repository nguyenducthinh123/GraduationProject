using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt
{
    public class ClientStatus
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        bool _connected;
        public bool IsConnected => _connected;

        public event Action ConnectionLost;
        public event Action Ready;
        public void SetConnectionState(bool connected)
        {
            if (_connected != connected)
            {
                _connected = connected;
                if (connected)
                {
                    Ready?.Invoke();
                }
                else
                {
                    ConnectionLost?.Invoke();
                }
            }
        }
    }
}
