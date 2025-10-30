using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Vst.MQTT
{
    public class Client
    {
        class Tcp
        {
            Socket _socket;

            static IPEndPoint _remoteEP;

            Queue<Packet> _packets = new Queue<Packet>();
            Packet _current;
            byte[] _data;

            public bool Busy => _current != null;
            public Action OnError = () => {
                Screen.Error("TCP error");
            };

            void _write(Packet packet)
            {
                if (packet != null)
                {
                    try
                    {
                        _current = null;
                        if (packet.ACK != 0)
                            _current = packet;

                        _data = packet.ToBytes();
                        _socket.Send(_data);

                    }
                    catch
                    {
                        OnError?.Invoke();
                    }
                }
            }


            public Tcp(string host, int port)
            {
                if (_remoteEP == null)
                {
                    IPAddress ip = null;
                    try
                    {
                        ip = IPAddress.Parse(host);
                    }
                    catch
                    {
                        try
                        {
                            ip = Dns
                                .GetHostEntry(host == null ? Dns.GetHostName() : host)
                                .AddressList[0];
                        }
                        catch (Exception ex)
                        {
                            Screen.Warning($"IP error: {ex.Message}");
                            return;
                        }
                    }

                    _remoteEP = new IPEndPoint(ip, port);
                }
            }
            public async Task<bool> TryConnect()
            {
                _socket = new Socket(_remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var connectTask = _socket.ConnectAsync(_remoteEP);
                var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(1000));

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    return false;
                }

                await connectTask;

                _packets.Clear();
                _socket.SendTimeout = 1000;

                return true;
            }
            public void Close(Action completed)
            {
                _socket.Shutdown(SocketShutdown.Both);
                completed?.Invoke();
            }

            public void Enqueue(Packet p) => _packets.Enqueue(p);

            public void Next()
            {
                if (!Busy && _packets.Count > 0)
                {
                    _write(_packets.Dequeue());
                }
            }
            public bool Read(Action<byte[]> callback)
            {

                if (_socket.Available == 0)
                    return false;


                var buffer = new byte[_socket.Available];
                _socket.Receive(buffer);
                _current = null;

                callback(buffer);
                return true;
            }

        }

        Tcp _tcp;
        Timer _clock;
        Counter _pingTicker = 0;

        #region CONSTRUCTORS
        public Client() { }
        public Client(string host, int port)
        {
            Host = host;
            Port = port;
        }
        public Client(string host) : this(host, 1883) { }
        #endregion

        #region CLIENT

        string _id;
        public string ID
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid().ToString();
                }
                return _id;
            }
            set => _id = value;
        }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int KeepAlive { get; set; } = 64; // second(s)
        #endregion

        #region PACKETS
        class Queue : Queue<Packet>
        {
            public bool IsEmpty => Count == 0;
        }
        #endregion

        #region SERVER
        public string Host { get; private set; } = "broker.emqx.io";
        public int Port { get; private set; } = 1883;
        void _analyse(byte[] data)
        {
            var code = data[0];
            switch (code)
            {
                case 0x00:
                    return;

                case 0x20:
                    IsConnected = true;
                    return;

                case 0x70: // disconnect (not work)
                    return;

                case 0xD0: // ping
                    return;
            }

            var len = new RemainingLength();
            var i = 1;
            while (i < data.Length && len.Read(data[i])) i++;

            _process_received_data(data, i + 1);
        }
        void _process_received_data(byte[] data, int index)
        {
            var code = data[0];
            if (code == 0x30)
            {
                int len = (data[index] << 8) | data[index + 1];
                string topic = Encoding.UTF8.GetString(data, index + 2, len);

                int i = index + len + 2;
                byte[] payload = new byte[data.Length - i];

                Array.Copy(data, i, payload, 0, payload.Length);

                RaiseDataRecieved(topic, payload);
                return;
            }
        }
        #endregion

        #region CONNECTION
        public bool IsConnected
        {
            get => _connected;
            private set
            {
                if (_connected != value)
                {
                    _connected = value;
                    if (_connected)
                        RaiseConnected();
                    else
                        RaiseConnectionLost();
                }
            }
        }
        bool _connected;

        /// <summary>
        /// Chu kỳ kiểm tra kết nối
        /// </summary>
        public Client SetCheckConnectionInterval(int seconds)
        {
            _pingTicker = seconds;
            return this;
        }
        public void Connect(int secondsTimeout = 10)
        {
            if (_connected) return;

            Task.Run(async () => {
                Counter cd = secondsTimeout;
                Console.Write($"Connecting to {Host} ");

                while (true)
                {
                    _tcp = new Tcp(Host, Port);

                    if (await _tcp.TryConnect())
                    {
                        _pingTicker.Reset();

                        _tcp.OnError = () => {
                            IsConnected = false;
                            if ((int)_pingTicker != 0)
                            {
                                Connect(secondsTimeout);
                            }    
                        };
                        _tcp.Enqueue(Packet.Connect(ID, UserName, Password, KeepAlive));

                        _clock = new Timer();
                        _clock.OnTick += () => {
                            _tcp.Read(_analyse);
                            _tcp.Next();
                        };

                        _clock.OnSecond += () => {
                            if (_connected && _pingTicker.CheckInterupt())
                            {
                                _tcp.Enqueue(Packet.Ping());
                            }
                        };
                        _clock.Start();

                        return;
                    }

                    Console.Write('.');
                    if (cd.CheckInterupt())
                    {
                        RaiseConnectError();
                        return;
                    }
                }
            });
        }
        
        public void Disconnect()
        {
            _tcp.Close(_clock.Stop);
            _connected = false;

            RaiseDisconnected();
        }
        #endregion

        #region EVENTS
        public event Action<string, byte[]> DataReceived;
        public event Action Connected;
        public event Action Disconnected;
        public event Action ConnectionLost;
        public event Action ConnectionError;
        protected virtual void RaiseConnectionLost()
        {
            ConnectionLost?.Invoke();
        }
        protected virtual void RaiseDisconnected()
        {
            Disconnected?.Invoke();
        }
        protected virtual void RaiseConnected()
        {
            Connected?.Invoke();
        }
        protected virtual void RaiseDataRecieved(string topic, byte[] payload)
        {
            DataReceived?.Invoke(topic, payload);
        }
        protected virtual void RaiseConnectError()
        {
            ConnectionError?.Invoke();
        }
        #endregion

        #region SUBSCRIBE
        public virtual void Subscribe(string topic, byte qos)
        {
            _tcp.Enqueue(Packet.Subscribe(topic, qos));
        }
        public void Subscribe(string topic)
        {
            Subscribe(topic, 0);
        }
        public void Unsubscribe(IEnumerable<string> topics)
        {
            foreach (var s in topics)
            {
                Unsubscribe(s);
            }
        }
        public virtual void Unsubscribe(string topic)
        {
            _tcp.Enqueue(Packet.Unsubcribe(topic));
        }
        #endregion

        #region PUBLISH
        public void Publish(string topic, byte[] message, byte qos, bool retain)
        {
            _tcp.Enqueue(Packet.Publish(topic, message, qos, retain));
        }
        public void Publish(string topic, byte[] message, byte qos)
        {
            Publish(topic, message, qos, false);
        }
        public void Publish(string topic, byte[] message)
        {
            Publish(topic, message, 0, false);
        }
        public void Publish(string topic, string message, byte qos, bool retain)
        {
            Publish(topic, Encoding.UTF8.GetBytes(message), qos, retain);
        }
        public void Publish(string topic, string message, byte qos)
        {
            Publish(topic, message, qos, false);
        }
        public void Publish(string topic, string message)
        {
            Publish(topic, message, 0, false);
        }
        #endregion
    }
}
