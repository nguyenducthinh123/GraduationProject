using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Screen
    {
        static public void WriteLine(string text)
        {
            Console.WriteLine(text);
            Console.ResetColor();
            Start();
        }
        static public void Start()
        {
        }
        static public void WriteLine(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            WriteLine(text);
        }
        static public void Warning(string text) => WriteLine(ConsoleColor.Yellow, text);
        static public void Error(string text) => WriteLine(ConsoleColor.Red, text);
        static public void Success(string text) => WriteLine(ConsoleColor.Green, text);
        static public void Info(string text) => WriteLine(ConsoleColor.Cyan, text);
        static public void Light(string text) => WriteLine(ConsoleColor.White, text);
        static public void Message(string text) => WriteLine(ConsoleColor.Gray, text);
        static public void Primary(string text) => WriteLine(ConsoleColor.Blue, text);
        static public void Love(string text) => WriteLine(ConsoleColor.Magenta, text);
        static public void Done() => Success("Done");

        static public void Waiting(string text, int seconds, Func<bool> func)
        {
            var clock = new Clock(10);
            clock.OneTick += () => { 
                if (func?.Invoke() == true)
                {
                    Console.Write(' ');
                    Done();

                    clock.Stop();
                    return;
                }
            };
            clock.OneSecond += () => { 
                if (clock.Second == seconds)
                {
                    Error(" Timeout");
                    clock.Stop();
                }
                else
                {
                    Console.Write('.');
                }
            };
            Console.Write(text);
            clock.Start();
        }

        static public void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }
        static public string Now(object message) => $"{DateTime.Now:HH:mm:ss} {message}";

        public static string Command { get; private set; }
        public static string[] Params { get; private set; }
        public static string GetParam(int index)
        {
            if (index >= Params.Length)
                return null;

            return Params[index];
        }
        public static string GetJsonParams(params string[] keys)
        {
            var s = new List<string>();
            for (int i = 0; i < keys.Length && i < Params.Length; i++)
            {
                s.Add($"'{keys[i]}':'{Params[i]}'");
            }

            return '{' + string.Join(", ", s) + '}';
        }

        public static void Loop(Action<string, string[]> callback)
        {
            Task.Run(() => {
                while (true)
                {
                    var items = Console.ReadLine().Trim().Split(' ');
                    Command = items[0];
                    if (Command == string.Empty)
                        continue;

                    var lst = new List<string>();
                    for (int i = 1; i < items.Length; i++)
                    {
                        var s = items[i].Trim();
                        if (s != string.Empty)
                            lst.Add(s);
                    }
                    Params = lst.ToArray();

                    callback(Command, Params);
                }
            });
        }
    }
}

namespace System
{
    public class Clock
    {
        public void Stop() { IsRunning = false; }
        public void Start()
        {
            IsRunning = true;
            Milis = 0;
            Second = 0;
            Minute = 0;
            Hour = 0;
            Day = 0;

            while (IsRunning)
            {
                Threading.Thread.Sleep(Interval);

                Milis += Interval;
                OneTick?.Invoke();

                if (Milis >= 1000)
                {
                    Milis -= 1000;

                    ++Second;
                    OneSecond?.Invoke();

                    if (Second >= 60)
                    {
                        Second = 0;

                        ++Minute;
                        OneMinute?.Invoke();

                        if (Minute >= 60)
                        {
                            Minute = 0;
                            
                            ++Hour;
                            OneHour?.Invoke();

                            if (Hour >= 24)
                            {
                                Hour = 0;

                                ++Day;
                                OneDay?.Invoke();
                            }
                        }
                    }
                }
            }
        }
        public void StartAsync()
        {
            if (!IsRunning)
            {
                Task.Run(Start);
            }
        }

        public bool IsRunning { get; set; }
        public int Milis { get; set; }
        public int Second { get; set; }
        public int Minute { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }

        public event Action OneTick;
        public event Action OneSecond;
        public event Action OneMinute;
        public event Action OneHour;
        public event Action OneDay;

        public int Interval { get; set; }
        public Clock(int interval)
        {
            Interval = interval;
        }
        public Clock() : this(100) { }
    }
}
