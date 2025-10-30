using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vst
{
    public class Counter
    {
        int _val;
        int _cur;

        public Counter(int value)
        {
            _val = _cur = value;
        }
        public bool CheckInterupt()
        {
            if (_val > 0)
            {
                _cur--;
                if (_cur == 0)
                {
                    _cur = _val;
                    return true;
                }
            }
            return false;
        }
        public void Reset() => _cur = _val;

        static public implicit operator Counter(int v) => new Counter(v);
        static public explicit operator int(Counter c) => c._val;
    }
    public class Timer
    {

        public event Action OnTick;
        public event Action OnSecond;

        public int Interval { get; set; } = 100;
        int _val;

        public bool IsAlive { get; private set; }
        public virtual void Start()
        {
            if (IsAlive) return;

            IsAlive = true;

            _val = 1000;
            Task.Run(async () => {
                while (IsAlive)
                {
                    await Task.Delay(Interval);

                    _val -= Interval;
                    RaiseOnTick();

                    if (0 >= _val)
                    {
                        _val = 1000;
                        RaiseOnSecond();
                    }
                }
            });
        }
        public virtual void Stop()
        {
            IsAlive = false;
        }

        protected virtual void RaiseOnTick() => OnTick?.Invoke();
        protected virtual void RaiseOnSecond() => OnSecond?.Invoke();
    }
}
