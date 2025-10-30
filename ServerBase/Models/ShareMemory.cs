using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;

namespace Vst.Server
{
    public class MemoryPacket
    {
        const int header_length = 3;

        public byte[] Data { get; private set; }
        int position;
        public MemoryPacket(string topic, byte[] payload)
        {
            int size = topic.Length + payload.Length + header_length;
            Data = new byte[size];

            Data[0] = (byte)size;
            Data[1] = (byte)(size >> 8);
            Data[2] = (byte)topic.Length;

            int i = header_length;
            foreach (char c in topic)
            {
                Data[i++] = (byte)c;
            }
            foreach (var b in payload)
            {
                Data[i++] = b;
            }
        }
        public MemoryPacket(byte[] data, int index)
        {
            Data = data;
            position = index;
        }

        public int Size => (Data[position + 1] << 8) | Data[position];
        public string Topic
        {
            get
            {
                int n = Data[position + 2];
                var v = new char[n];
                var k = header_length + position;
                for (int i = 0; i < n; i++)
                    v[i] = (char)Data[k++];

                return new string(v);
            }
        }
        public byte[] Payload
        {
            get
            {
                int n = Data[position + 2];
                var k = header_length + n;
                var v = new byte[Size - k];

                k += position;
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = Data[k++];
                }
                return v;
            }
        }
        public string Message => Payload.UTF8();
        public Document Document => Document.Parse(Message);
    }
}

namespace Vst.Server
{
    using SE = Encoding;
    using MMVS = MemoryMappedViewStream;
    public static class MemoryExtended
    {
        static public MMVS Seek(this MMVS m, long position)
        {
            m.Position = position;
            return m;
        }
        static public long GetValue(this MMVS m, int size)
        {
            long v = 0;
            for (int i = 0; i < size; i++)
            {
                v |= (long)(m.ReadByte() << (i << 3));
            }
            return v;
        }
        static public MMVS SetValue(this MMVS m, long value, int size)
        {
            for (int i = 0; i < size; i++, value >>= 8)
            {
                m.WriteByte((byte)value);
            }
            return m;
        }

        static public byte[] Read(this MMVS m, int length)
        {
            var bytes = new byte[length];
            m.Read(bytes, 0, length);
            return bytes;
        }
        static public MMVS Write(this MMVS m, byte[] value)
        {
            m.Write(value, 0, value.Length);
            return m;
        }
    }

    public class MemoryHeader
    {
        MMVS stream;
        public MemoryHeader(MMVS src)
        {
            stream = src;
        }

        public long Size { get; set; } = 1024;
        public int DataLength
        {
            get => (int)stream.Seek(0).GetValue(4);
            set => stream.Seek(0).SetValue(value, 4);
        }
    }
    public partial class Memory : IDisposable
    {
        Mutex _mutex;

        string _name;
        string _mutext_name;
        public string Name => _name;
        public MemoryHeader Header { get; private set; }

        public Memory(string name)
        {
            _name = name.ToLower();
            _mutext_name = _name + "-mutext";
        }
        public Memory Create(int MB)
        {
            try
            {
                if (!Exists)
                {
                    bool b;

                    _mutex = new Mutex(true, _mutext_name, out b);
                    _mutex.ReleaseMutex();
                    
                    MemoryMappedFile.CreateNew(_name, MB << 20);
                }

            }
            catch
            {
            }
            return this;
        }

        public bool Exists
        {
            get => Mutex.TryOpenExisting(_mutext_name, out _mutex);
        }

        public event Action<Memory, MemoryMappedFile> OnError;
        protected void Open(Action<MemoryMappedFile> callback)
        {
            if (!Exists)
            {
                Screen.Error($"{Name} not found");
                return;
            }

            MemoryMappedFile mmf = null;
            try
            {
                mmf = MemoryMappedFile.OpenExisting(_name);
            }
            catch (Exception e)
            {
                Screen.Error(e.Message);
                OnError?.Invoke(this, mmf);

                return;
            }
            callback?.Invoke(mmf);
        }
        public void Dispose()
        {
            Open(_mmf => {
                _mmf.Dispose();
                _mutex.Dispose();
            });
        }
        public void Streaming(Action<MMVS> callback)
        {
            this.Open(mmf => {
                _mutex.WaitOne();

                using (var stream = mmf.CreateViewStream())
                {
                    Header = new MemoryHeader(stream);
                    callback.Invoke(stream);
                }
                _mutex.ReleaseMutex();
            });
        }
    }
    public class ShareMemory : Memory
    {
        public ShareMemory(string name) : base(name) { }
        public List<MemoryPacket> ReadPackets()
        {
            List<MemoryPacket> lst = null;
            Streaming(s => {

                var len = Header.DataLength;
                if (len == 0) { return; }

                var buff = s.Seek(Header.Size).Read(len);
                Header.DataLength = 0;

                try
                {
                    int i = 0;
                    lst = new List<MemoryPacket>();

                    while (i < len)
                    {
                        var m = new MemoryPacket(buff, i);
                        lst.Add(m);

                        i += m.Size;
                    }
                }
                catch
                {
                    Console.WriteLine("*** Share Memory Read Packet ***\n");
                }
            });
            return lst;
        }
        public bool WritePacket(MemoryPacket m)
        {
            return WriteBytes(m.Data);
        }
        public bool WriteBytes(byte[] v)
        {
            bool r = false;
            Streaming(s => {

                int len = Header.DataLength;
                
                s.Seek(Header.Size + len).Write(v);
                Header.DataLength = v.Length + len;

                r = true;
            });
            return r;
        }
        public bool WriteObject(object value)
        {
            if (value == null)
            {
                Screen.Warning("Can not write an empty object");
                return false;
            }

            return WriteBytes(SE.UTF8.GetBytes(value.ToString()));
        }
        
        bool CheckProcessRunning(MMVS s)
        {
            var buf = new byte[1];
            s.Position = 4;
            s.Read(buf, 0, buf.Length);
            return buf[0] != 0;
        }
        public bool IsProcessRunning
        {
            get 
            {
                bool b = false;
                Streaming(s => b = CheckProcessRunning(s));
                
                return b;
            }
            set
            {
                var buf = new byte[] { (byte)(value ? 1 : 0) };
                Streaming(s => {
                    s.Position = 4;
                    s.Write(buf, 0, buf.Length);
                });
            }
        }    
        public ProcessInfo ProcessInfo
        {
            get
            {
                ProcessInfo p = null;
                Streaming(s => {
                    var len = (int)s.Seek(5).GetValue(2);
                    var buf = s.Read(len);

                    p = Document.Parse(SE.UTF8.GetString(buf)).ChangeType<ProcessInfo>();
                });
                return p;
            }
            set
            {
                Streaming(s => {
                    var dat = value.ToString();
                    var buf = SE.UTF8.GetBytes(dat);
                    s.Seek(5).SetValue(buf.Length, 2).Write(buf);
                });
            }
        }

        static public void Open(string name, Action<ShareMemory> callback)
        {
            var sm = new ShareMemory(name);
            sm.Open(mmf => {
                callback?.Invoke(sm);
            });
        }
    }
}

namespace Vst.Server
{
    partial class Memory
    {
        static public ShareMemory Master => new ShareMemory(nameof(Master));
        static public ShareMemory Monitoring => new ShareMemory(nameof(Monitoring));
        static public ShareMemory Broker => new ShareMemory(nameof(Broker));
        static public ShareMemory Account => new ShareMemory(nameof(Account));
        static public ShareMemory Firmware => new ShareMemory(nameof(Firmware));
    }
}