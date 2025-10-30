using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTGO
{
    public class Channels : List<Sim>
    {
        public event Action<Sim> ChannelDetected;
        public void Detect()
        {
            var portNames = System.IO.Ports.SerialPort.GetPortNames();
            if (portNames.Length == 0)
            {
                Screen.Error("Port not found");
                return;
            }
            foreach (var name in portNames)
            {
                var uart = new UART(name);
                //uart.Responsed += (s) => {
                //    if (s == "OK")
                //    {
                //        uart.Close();

                //        var sim = new Sim(name);
                //        sim.Start();

                //        Add(sim);
                //        ChannelDetected?.Invoke(sim);
                //    }
                //};

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
    }

    public class SimCommand
    {
        public string Text { get; set; }
        public int Duration { get; set; } // seconds
    }

    public class Sim
    {
        UART _port;
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
            var cmd = new SimCommand { 
                Text = $"SMS({correctNumber(number)},\"{text.VnCharacter()}\")",
                Duration = 6,
            };
            Commands.Enqueue(cmd);
        }
        public void CreateCALL(string number)
        {
            var cmd = new SimCommand { 
                Text = $"CAL({correctNumber(number)})",
                Duration = 10,
            };
            Commands.Enqueue(cmd);
            Commands.Enqueue(new SimCommand { 
                Text = "END()",
                Duration = 3,
            });
        }

        int count_down = 0;
        SimCommand _current = null;

        public void Execute()
        {
            if (count_down > 0)
            {
                --count_down;
                return;
            }

            if (Commands.Count > 0)
            {
                _current = Commands.Dequeue();

                _port.WriteCommand(_current.Text);
                count_down = _current.Duration;
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
