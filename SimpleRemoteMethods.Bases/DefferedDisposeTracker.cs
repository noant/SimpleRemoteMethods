using System;
using System.Collections.Generic;

namespace SimpleRemoteMethods.Bases
{
    internal class DefferedDisposeTracker
    {
        private readonly Dictionary<IDisposable, uint> _objects = new Dictionary<IDisposable, uint>();
        private readonly List<IDisposable> _demand = new List<IDisposable>();

        public void BeginUse(IDisposable obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (_demand.Contains(obj))
            {
                throw new InvalidOperationException("Object marked as demand to dispose");
            }

            lock (obj)
            {
                Increment(obj);
            }
        }

        public void EndUse(IDisposable obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var count = GetCount(obj);
            if (count == 0)
            {
                throw new InvalidOperationException("Object is not in use");
            }

            lock (obj)
            {
                count = Decrement(obj);
                if (count == 0 && _demand.Contains(obj))
                {
                    lock (_demand)
                    {
                        _demand.Remove(obj);
                    }
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} {obj} is disposed.");
#endif
                    obj.Dispose();
                }
            }
        }

        public void DisposeWhenUseIsComplete(IDisposable obj)
        {
            lock (_demand)
            {
                if (GetCount(obj) == 0)
                {
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} {obj} is disposed.");
#endif
                    obj.Dispose();
                }
                else
                {
#if DEBUG
                    Console.WriteLine($"{DateTime.Now} {obj} is marked to dispose.");
#endif
                    _demand.Add(obj);
                }
            }
        }

        private uint GetCount(IDisposable obj)
        {
            if (!_objects.ContainsKey(obj))
            {
                return 0;
            }
            else
            {
                return _objects[obj];
            }
        }

        private void Increment(IDisposable obj)
        {
            if (!_objects.ContainsKey(obj))
            {
                _objects.Add(obj, 1);
            }
            else
            {
                _objects[obj]++;
            }
        }

        private uint Decrement(IDisposable obj)
        {
            if (!_objects.ContainsKey(obj))
            {
                throw new InvalidOperationException("Object is not in use");
            }

            var cnt = --_objects[obj];
            if (cnt == 0)
            {
                _objects.Remove(obj);
            }

            return cnt;
        }
    }
}