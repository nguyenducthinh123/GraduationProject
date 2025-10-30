using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace Vst.Server
{
    public class ProcessInfo : Document
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public string Path
        {
            get => GetString("path");
            set => Push("path", value);
        }
        public string FileName
        {
            get => GetString("fileName");
            set => Push("fileName", value);
        }
        public int MemorySize
        {
            get 
            {
                var sz = GetValue<int>(nameof(MemorySize));
                if (sz == 0)
                    MemorySize = sz = 1;
                return sz;
            }
            set => Push(nameof(MemorySize), value);
        }
        public int ThreadInterval
        {
            get 
            { 
                var it = GetValue<int>(nameof(ThreadInterval));
                if (it == 0)
                    ThreadInterval = it = 10;
                return it;
            }
            set => Push(nameof(ThreadInterval), value);
        }

        public string FullPath
        {
            get 
            {
                var dir = Path;
                if (dir != null)
                    dir += '\\';
                return $"{dir}{FileName}";
            }
            set
            {
                var i = value.LastIndexOf('\\');
                Path = value.Substring(0, i);

                var name = value.Substring(i + 1);
                FileName = name;

                var words = new List<string>();
                var n = 0;
                var id = "";

                i = 0;
                foreach (var c in name)
                {
                    if (c == '.') break;

                    id += c;

                    if (char.IsUpper(c) && n > 0)
                    {
                        words.Add(name.Substring(i, n));
                        i += n;
                        n = 0;
                    }
                    ++n;
                }
                if (n > 0) words.Add(name.Substring(i, n));

                if (Name == null)
                {
                    Name = string.Join(" ", words);
                }
                ObjectId = id;
            }
        }

        bool _hidden;
        public bool Hidden 
        {
            get => _hidden; 
            set
            {
                if (_hidden != value && _process != null)
                {
                    _hidden = value;
                    ShowWindow(_process.MainWindowHandle, value ? SW_HIDE : SW_SHOW);

                    VisibleChanged?.Invoke(!value);
                }
            }
        }

        Process _process;

        public event Action<bool> VisibleChanged;
        public void ForEach(Action<Process> callback)
        {
            foreach (Process p in Process.GetProcessesByName(ObjectId))
            {
                callback(p);
            }
        }
        public bool IsFileExist => System.IO.File.Exists(FullPath);

        bool _started;

        public void SaveConfig()
        {
            System.IO.File.WriteAllText($"{Path}/{ObjectId}.json", this.ToString());
        }
        public virtual bool Start()
        {
            Process current = null;
            ForEach(p => {
                if (current == null)
                {
                    current = p;
                }
                else
                {
                    p.Kill();
                }
            });
            if (current != null)
            {
                return false;
            }

            if (IsFileExist == false)
                return false;

            var info = new ProcessStartInfo {
                FileName = FullPath,
                UseShellExecute = false,
            };

            _process = Process.Start(info);
            _process.EnableRaisingEvents = true;

            _started = true;
            return true;
        }
        public virtual void Stop()
        {
            _started = false;
            try
            {
                ForEach(p => p.Kill());
            }
            catch
            {
            }
        }
        public virtual bool IsAlive
        {
            get
            {
                bool b = false;
                ForEach(p => b = true);
                return b;
            }
        }
        public bool IsKilled => _started && !IsAlive;
    }
}
