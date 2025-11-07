using System.Collections.Concurrent;

namespace Multiplayer
{
    internal class Dispatcher
    {
        public delegate void DispatcherCallbackDelegate();

        private readonly List<Exception> dispatcherExceptions = new();
        private readonly ConcurrentQueue<DispatcherCallbackDelegate> _threadQueue = new();
        public void Update()
        {
            dispatcherExceptions.Clear();
            while (_threadQueue.TryDequeue(out var f))
            {
                try
                {
                    f?.Invoke();
                }
                catch (Exception ex)
                {
                    dispatcherExceptions.Add(ex);
                }
            }
            if (dispatcherExceptions.Count != 0)
            {
                throw new AggregateException("Caught one or more errors while invoking callbacks", dispatcherExceptions);
            }
        }
        public void Enqueue(DispatcherCallbackDelegate del)
        {
            _threadQueue.Enqueue(del);
        }
    }
}
