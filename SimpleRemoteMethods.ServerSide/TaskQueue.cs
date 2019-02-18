using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.ServerSide
{
    public class TaskQueue
    {
        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
        private volatile ushort _currentThreadsCount = 0;

        public TaskQueue(ushort limit = 4)
        {
            if (limit == 0)
            {
                throw new ArgumentOutOfRangeException("Limit must be more than 0");
            }

            TasksLimit = limit;
        }

        public ushort TasksLimit { get; }

        public void Enqueue(Action action)
        {
            _actions.Enqueue(action);
            StartTasks();
        }

        private void StartTasks()
        {
            lock (_actions)
            {
                if (_currentThreadsCount < TasksLimit)
                {
                    Task.Run(() => TaskProcess());
                }
            }
        }

        private void TaskProcess()
        {
            _currentThreadsCount++;
            try
            {
                while (_actions.TryDequeue(out Action action))
                {
                    action();
                }
            }
            finally
            {
                _currentThreadsCount--;
            }
        }
    }
}