using System.Collections.Concurrent;

namespace Multiplayer.Static
{
    internal class Dispatcher
    {
        internal delegate void DispatcherCallbackDelegate();
        internal readonly ConcurrentQueue<DispatcherCallbackDelegate> ThreadQueue = new();

        internal void Update()
        {
            while (ThreadQueue.TryDequeue(out var f))
            {
                try
                {
                    f?.Invoke();
                }
                catch (Exception ex)
                {
                    Main.Log(ex);
                }
            }
        }
    }
}
