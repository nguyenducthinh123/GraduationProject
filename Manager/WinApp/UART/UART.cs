using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Channels : List<Sim>
    {
        public event Action<Sim> ChannelDetected;
        public void Detect(Action<Channels> done)
        {
            this.Clear();

            var portNames = System.IO.Ports.SerialPort.GetPortNames();
            if (portNames.Length > 0)
            {
                foreach (var name in portNames)
                {
                    var uart = new UART(name);
                    uart.WriteCommand("STA()");

                    Screen.Waiting($"Detect {name} ...", 1, () => {
                        var s = uart.ReadExisting();

                        if (s.Contains("OK"))
                        {
                            uart.Close();

                            var sim = new Sim(name);
                            Add(sim);

                            ChannelDetected?.Invoke(sim);
                            return true;
                        }
                        return false;
                    });

                    uart.Close();
                }
            }

            done?.Invoke(this);
        }
    }

    public class SimCommand : Vst.Counter
    {
        public SimCommand(int duration, string name, string text = null) : base(duration) {
            Name = name;
            Text = text;
        }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            var s = Name + '(';
            if (Number != null) s += Number;
            if (Text != null) s += ',' + Text;

            return s + ')';
        }
    }

    public class Sim
    {
        static public Channels Channels { get; private set; } = new Channels();

        static Sim _demo;
        static public Sim DefaultChannel
        {
            get
            {
                if (Channels.Count == 0)
                {
                    if (_demo == null)
                        _demo = new Sim("#");
                    return _demo;
                }

                return Channels[0];
            }
        }


        UART _port;
        public string PortName => _port.PortName;
        public Sim(string name)
        {
            _port = new UART(name);
        }
        public Queue<SimCommand> Commands { get; private set; } = new Queue<SimCommand>();

        string correctNumber(string number)
        {
            var v = number.ToCharArray();
            int k = 0;
            for (int i = 0; i < v.Length; i++)
            {
                char c = v[i];
                if (c == '+')
                {
                    i += 2;
                    continue;
                }
                if (char.IsDigit(c))
                    v[k++] = c;
            }
            return new string(v, 0, k);
        }
        public void CreateSMS(string number, string text)
        {
            var cmd = new SimCommand(6, "SMS", text.VnCharacter());
            cmd.Number = correctNumber(number);
            Commands.Enqueue(cmd);
        }
        public void CreateCALL(string number)
        {
            var cmd = new SimCommand(15, "CAL");
            cmd.Number = correctNumber(number);

            Commands.Enqueue(cmd);
            Commands.Enqueue(new SimCommand(3, "END"));
        }

        SimCommand _current = null;

        public event Action<Sim, SimCommand> OnSending; 
        public void Execute()
        {
            if (_current != null && _current.CheckInterupt() == false)
                return;

            if (Commands.Count > 0)
            {
                _current = Commands.Dequeue();
                
                switch (_current.Name)
                {
                    case "SMS":
                    case "CAL":
                        OnSending?.Invoke(this, _current);
                        break;
                }

                var msg = _current.ToString();
                if (PortName != "#")
                {
                    _port.WriteCommand(msg);
                }
            }
        }
    }

    public class UART : System.IO.Ports.SerialPort, IDisposable
    {
        public string Buffer { get; private set; } = string.Empty;
        public const string CRLF = "\r\n";

        public UART(string name) : this() { PortName = name; }
        public UART()
        {
            this.PortName = "unknown";
            this.ReadTimeout = 3000;
        }
        public void WriteCommand(string command)
        {
            Screen.Warning(command);
            try
            {
                if (IsOpen == false)
                    Open();
                DiscardOutBuffer();
                DiscardInBuffer();

                Write(command + CRLF);
            }
            catch (Exception ex)
            {
                Screen.Error(ex.Message);
            }
        }
    }
}
