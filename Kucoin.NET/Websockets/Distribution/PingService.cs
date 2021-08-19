
using Kucoin.NET.Data.User;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Kucoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Provides automatic pinging services for objects that must ping servers at regular intervals.
    /// </summary>
    public static class PingService
    {
        private static object lockObj = new object();

        private static readonly List<IPingable> feeds = new List<IPingable>();

        private static Thread IntervalThread;

        private static CancellationTokenSource cts;

        private static bool changed = false;

        public const int Wait = 10;

        static PingService()
        {
        }

        /// <summary>
        /// Register a new pingable object.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object was successfully registered, false if the object was already registered.</returns>
        public static bool RegisterService(IPingable obj)
        {
            lock(lockObj)
            {
                if (!feeds.Contains(obj))
                {
                    int x = obj.Interval;

                    if (x % Wait != 0)
                    {
                        x = x - (x % Wait);
                    }

                    obj.Interval = x;
                    if (x == 0) throw new ArgumentOutOfRangeException("Interval cannot be 0.");

                    feeds.Add(obj);
                    changed = true;
                }
                else
                {
                    return false;
                }

                if (IntervalThread == null)
                {
                    cts = new CancellationTokenSource();

                    IntervalThread = new Thread(IntervalMethod);
                    IntervalThread.IsBackground = true;
                    IntervalThread.Start();
                }

                return true;
            }
        }

        /// <summary>
        /// Unregister a pingable object.
        /// </summary>
        /// <param name="obj">The object to unregister.</param>
        /// <returns>True if the object was successfully unregistered, false if the object was not registered.</returns>
        public static bool UnregisterService(IPingable obj)
        {
            lock (lockObj)
            {
                if (feeds.Contains(obj))
                {
                    feeds.Remove(obj);
                    changed = true;
                }
                else
                {
                    return false;
                }

                if (feeds.Count == 0)
                {
                    CancelIntervalThread();
                }

                return true;
            }
        }

        /// <summary>
        /// Cancel the pinger thread.
        /// </summary>
        private static void CancelIntervalThread()
        {
            cts?.Cancel();

            IntervalThread = null;
            cts = null;
        }

        /// <summary>
        /// Pinger method.
        /// </summary>
        private static void IntervalMethod()
        {
            List<IPingable> stash = new List<IPingable>();
            List<int> counts = new List<int>();
            int c = 0, i;
            IPingable feed;
            int wait = Wait;

            while (!cts?.IsCancellationRequested ?? false)
            {
                try
                {
                    if (changed)
                    {
                        lock (lockObj)
                        {
                            changed = false;

                            stash.Clear();
                            counts.Clear();

                            stash.AddRange(feeds);
                            stash.ForEach((a) => counts.Add(0));

                            c = feeds.Count;
                        }
                    }

                    for (i = 0; i < c; i++)
                    {
                        feed = feeds[i];
                        counts[i] += wait;

                        if (counts[i] == feed.Interval)
                        {
                            counts[i] = 0;

                            if (feed is IAsyncPingable aping)
                            {
                                _ = aping.Ping();
                            }
                            else
                            {
                                feed.Ping();
                            }
                        }

                    }

                    Thread.Sleep(wait);

                    if (cts?.IsCancellationRequested ?? true) return;
                }
                catch //(Exception ex)
                {
                    
                }

            }
        }
        

    }
}
