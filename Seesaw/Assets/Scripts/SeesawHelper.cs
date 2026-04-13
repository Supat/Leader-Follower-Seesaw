using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SeesawHelper
{
    public static class LSLStreamHelper
    {
        public static string ResolveStreamSuffix(int playerID)
        {
            if (playerID == 1)
                return "Server";
            else if (playerID == -1)
                return "Client";
            else
                return "";
        }
    }

    public static class PlayerHelper
    {
        public static float BasePlayerSpeed { get; } = 0.02f;
        public static float MaxAcceleration { get; } = 2.5f;
        public static float AccelerationRate { get; } = 0.03f;
    }

    public class FixedSizedQueue<T>
    {
        readonly ConcurrentQueue<T> q = new ConcurrentQueue<T>();
        private readonly object lockObject = new object();

        public int Limit { get; set; }

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                while (q.Count > Limit && q.TryDequeue(out _)) ;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return q.GetEnumerator();
        }
    }
}
