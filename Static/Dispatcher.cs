using System.Collections.Concurrent;

namespace Multiplayer.Static
{
    internal class Dispatcher
    {
        public delegate void DispatcherCallbackDelegate();

        private readonly List<Exception> DispatcherExceptions = new();
        private readonly ConcurrentQueue<DispatcherCallbackDelegate> _threadQueue = new();
        public void Update()
        {
            DispatcherExceptions.Clear();
            while (_threadQueue.TryDequeue(out var f))
            {
                try
                {
                    f?.Invoke();
                }
                catch (Exception ex)
                {
                    DispatcherExceptions.Add(ex);
                }
            }
            if (DispatcherExceptions.Count != 0)
            {
                throw new AggregateException("Caught one or more errors while invoking callbacks", DispatcherExceptions);
            }
        }
        public void Enqueue(DispatcherCallbackDelegate del)
        {
            _threadQueue.Enqueue(del);
        }
    }
}
