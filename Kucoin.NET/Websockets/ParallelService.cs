using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kucoin.NET.Websockets
{


    public interface IDistributable // : IComparable<IDistributable>
    {
        /// <summary>
        /// Do the work for this tenant.
        /// </summary>
        /// <returns>True if work was done, otherwise false.</returns>
        bool DoWork();

        object LockObject { get; }

    }

    public static class ParallelService
    {
        private class Distribution : IDisposable
        {
            bool disposed;
            
            private CancellationTokenSource cts = new CancellationTokenSource();

            public List<IDistributable> Tenants { get; } = new List<IDistributable>();

            public Thread Thread { get; private set; }

            public void Dispose()
            {
                if (disposed) throw new ObjectDisposedException(GetType().FullName);

                cts?.Cancel();
                Thread = null;
                cts = null;
                disposed = true;
            }

            public Distribution()
            {
                Thread = new Thread(ThreadMethod);
                Thread.IsBackground = true;
            }

            private void ThreadMethod()
            {
                while (!cts.IsCancellationRequested)
                {
                    int i = 0;

                    lock (Tenants)
                    {
                        foreach (var t in Tenants)
                        {
                            if (!t.DoWork()) i++;
                        }
                    }

                    Thread.Sleep(i * IdleSleepTime);
                }
            }

            public Distribution(IDistributable tenant) : this()
            {
                Tenants.Add(tenant);
            }
        }

        private static object lockObj = new object();

        private static readonly List<Distribution> distributors = new List<Distribution>();
                        
        public static int MaxTenants { get; private set; } = 6;

        public static int IdleSleepTime { get; set; } = 1;

        public static void RegisterService(IDistributable feed)
        {
            lock (lockObj)
            {
                if (distributors.Where((a) => a.Tenants.Where((b) => b.Equals(feed)).Count() != 0).Count() == 0)
                {
                    var insertFeed = distributors.Where((a) => a.Tenants.Count < MaxTenants).FirstOrDefault();

                    if (insertFeed == default)
                    {
                        insertFeed = new Distribution(feed);
                        insertFeed.Thread.Start();

                        distributors.Add(insertFeed);
                    }
                    else
                    {
                        lock(insertFeed.Tenants)
                        {
                            insertFeed.Tenants.Add(feed);
                        }
                    }

                }
            }
        }

        public static void RedistributeServices(int distribution)
        {
            lock (lockObj)
            {
                if (distribution < 1 || distribution > 255) throw new ArgumentOutOfRangeException(nameof(distribution) + " must be 1 to 255");

                var allfeeds = from dist in distributors from feed in dist.Tenants select feed;

                foreach (var feed in allfeeds)
                {
                    Monitor.Enter(feed.LockObject);
                }

                foreach (var dist in distributors)
                {
                    dist.Dispose();
                }

                distributors.Clear();

                int x = 0;

                foreach (var feed in allfeeds)
                {
                    RegisterService(feed);
                }

                foreach (var feed in allfeeds)
                {
                    Monitor.Exit(feed.LockObject);
                }

            }

        }


        public static void UnregisterService(IDistributable feed)
        {
            lock (lockObj)
            {
                var distributor = distributors.Where((a) => a.Tenants.Contains(feed)).FirstOrDefault();

                lock(distributor.Tenants)
                {
                    distributor.Tenants.Remove(feed);

                    if (distributor.Tenants.Count == 0)
                    {
                        distributor.Dispose();
                        distributors.Remove(distributor);
                    }
                }
            }
        }


    }
}
