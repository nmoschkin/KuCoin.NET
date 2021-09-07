using Kucoin.NET.Data.Market;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Distribution.Services
{
    /// <summary>
    /// Organize distributable objects that do work in parallel.
    /// </summary>
    public static class ParallelService
    {
        private class Distribution : IDisposable
        {
            bool disposed;

            private CancellationTokenSource cts = new CancellationTokenSource();

            public List<IDistributable> Tenants { get; } = new List<IDistributable>();

            public bool ActionsChanged { get; set; } = true;

            public Thread Thread { get; private set; }

            public void Dispose()
            {
                if (disposed) throw new ObjectDisposedException(GetType().FullName);

                cts?.Cancel();
                cts = null;
                Thread = null;
                disposed = true;
            }

            public Distribution()
            {
                Thread = new Thread(ThreadMethod);
                Thread.IsBackground = true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThreadMethod()
            {
                var actions = new List<Action>();
                CancellationToken tok = cts.Token;

                Action[] arrActions = new Action[0];
                int x = 0, f = 0;

                while (!tok.IsCancellationRequested)
                {
                    if (ActionsChanged)
                    {
                        lock (Tenants)
                        {
                            ActionsChanged = false;

                            actions.Clear();

                            foreach (var t in Tenants)
                            {
                                actions.Add(t.DoWork);
                                //actions.Add(() =>
                                //{
                                //    t.DoWork();
                                //    Thread.Sleep(0);
                                //    t.DoWork();
                                //});
                            }

                            arrActions = actions.ToArray();
                        }
                    }

                    Parallel.Invoke(arrActions);

                    if (++f == sleepDivisor)
                    {
                        Thread.Sleep(idleSleepTime);
                        f = 0;
                    }

                }
            }

            public Distribution(IDistributable tenant) : this()
            {
                Tenants.Add(tenant);
            }
        }

        private static object lockObj = new object();

        private static readonly List<Distribution> distributors = new List<Distribution>();

        private static int maxTenants = 4;

        private static int idleSleepTime = 1;

        private static int sleepDivisor = 1;

        private static int workRepeat = 4;

        private static ThreadPriority distributorPriority = ThreadPriority.AboveNormal;

        /// <summary>
        /// Gets or sets the maximum number of feeds per thread.
        /// </summary>
        /// <remarks>
        /// Must be a value between 1 and 255.
        /// </remarks>
        public static int MaxTenants
        {
            get => maxTenants;
            set
            {
                RedistributeServices(value);
            }
        }

        public static int WorkRepeat
        {
            get => workRepeat;
            set
            {
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException();

                if (workRepeat != value)
                {
                    workRepeat = value;
                }
            }
        }

        public static ThreadPriority DistributorPriority
        {
            get => distributorPriority;
            set
            {
                if (value != distributorPriority)
                {
                    distributorPriority = value;

                    lock (lockObj)
                    {
                        foreach (var dist in distributors)
                        {
                            dist.Thread.Priority = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The thread will sleep for <see cref="IdleSleepTime"/> every <see cref="SleepDivisor"/> cycles.
        /// </summary>
        public static int SleepDivisor
        {
            get => sleepDivisor;
            set
            {
                if (value < 0 || value > 10000) throw new ArgumentOutOfRangeException();
                sleepDivisor = value;

            }
        }

        /// <summary>
        /// Gets or sets the global idle sleep time in milliseconds.
        /// </summary>
        /// <remarks>
        /// Must be a value between 0 and 100.<br /><br />
        /// The default value is 1.
        /// </remarks>
        public static int IdleSleepTime
        {
            get => idleSleepTime;
            set
            {
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException();
                idleSleepTime = value;
            }
        }

        /// <summary>
        /// Register an instance of a distributable service.
        /// </summary>
        /// <param name="feed">The service to register.</param>
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
                        insertFeed.Thread.Priority = distributorPriority;
                        insertFeed.Thread.Start();

                        distributors.Add(insertFeed);
                    }
                    else
                    {
                        lock (insertFeed.Tenants)
                        {
                            insertFeed.Tenants.Add(feed);
                            insertFeed.ActionsChanged = true;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Redistribute services so that each thread contains the specified number of <see cref="IDistributable"/> tenants.
        /// </summary>
        /// <param name="maxTenants">The maximum number of tenants per thread.</param>
        public static void RedistributeServices(int maxTenants)
        {
            lock (lockObj)
            {
                if (maxTenants < 1 || maxTenants > 255) throw new ArgumentOutOfRangeException(nameof(maxTenants) + " must be 1 to 255");

                var allfeeds = from dist in distributors from feed in dist.Tenants select feed;

                foreach (var dist in distributors)
                {
                    dist.Dispose();
                }

                distributors.Clear();
                ParallelService.maxTenants = maxTenants;

                foreach (var feed in allfeeds)
                {
                    Monitor.Enter(feed.LockObject);
                }

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

        /// <summary>
        /// Unregister an instance of a distributable service.
        /// </summary>
        /// <param name="feed">The service to unregister.</param>
        public static void UnregisterService(IDistributable feed)
        {
            lock (lockObj)
            {
                var distributor = distributors.Where((a) => a.Tenants.Contains(feed)).FirstOrDefault();

                lock (distributor.Tenants)
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
