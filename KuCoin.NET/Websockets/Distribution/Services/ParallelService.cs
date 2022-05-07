using KuCoin.NET.Data.Market;
using KuCoin.NET.Websockets.Public;

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

namespace KuCoin.NET.Websockets.Distribution.Services
{
    /// <summary>
    /// Organize and run workers on distributable objects that do work in parallel.
    /// </summary>
    public static class ParallelService
    {
        private class Distribution : IDisposable
        {
            bool disposed;

            bool zero = true;
            bool many = false;

            private CancellationTokenSource cts = new CancellationTokenSource();

            public List<IDistributable> Tenants { get; } = new List<IDistributable>();

            public bool ActionsChanged { get; set; } = true;

            public Thread Thread { get; private set; }

            private List<Action> actions;

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
                actions = new List<Action>();
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

                            f = Tenants.Count;

                            if (f == 0)
                            {
                                zero = true;
                                many = false;
                                return;
                            }
                            if (f == 1)
                            {
                                zero = false;
                                many = false;
                            }
                            else
                            {
                                zero = false;
                                many = true;
                                for (x = 0; x < f; x++)
                                {
                                    IDistributable t = Tenants[x];

                                    if (workRepeat > 1)
                                    {
                                        actions.Add(() =>
                                        {
                                            for (int r = 1; r <= workRepeat; r++)
                                            {
                                                t.DoWork();

                                                if (r == workRepeat) break;
                                                if (workIdleSleepTime < 0) continue;

                                                Thread.Sleep(workIdleSleepTime);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        if (workIdleSleepTime < 0)
                                        {
                                            actions.Add(t.DoWork);
                                        }
                                        else
                                        {
                                            actions.Add(() =>
                                            {
                                                t.DoWork();
                                                Thread.Sleep(workIdleSleepTime);
                                            });
                                        }
                                       
                                    }
                                }
                            }

                            f = 0;
                            arrActions = actions.ToArray();
                            continue;
                        }
                    }

                    lock (lockObj)
                    {
                        if (Tenants.Count == 0)
                        {
                            Thread.Sleep(5);
                        }
                        else if (many)
                        {
                            Parallel.Invoke(arrActions);
                            if (sleepDivisor < 0) continue;
                        }
                        else
                        {
                            Tenants[0].DoWork();
                        }
                    }

                    if (f == sleepDivisor)
                    {
                        Thread.Sleep(idleSleepTime);
                        f = 0;
                    }
                    else
                    {
                        f++;
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

        private static int sleepDivisor = 50;

        private static int workRepeat = 2;

        private static int workIdleSleepTime = 1;

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

        /// <summary>
        /// The thread will sleep for <see cref="IdleSleepTime"/> every <see cref="SleepDivisor"/> cycles.
        /// </summary>
        /// <remarks>
        /// Set this value to 0 to execute an idle sleep on every cycle.<br />
        /// Values less than 0 are not allowed.<br />
        /// The default value is 50.
        /// </remarks>
        public static int SleepDivisor
        {
            get => sleepDivisor;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException();
                sleepDivisor = value;
                RefreshAll();
            }
        }

        /// <summary>
        /// Gets or sets amount of time to sleep for each idle sleep, in milliseconds.
        /// </summary>
        /// <remarks>
        /// A value of 0 will yield the remainder of the time slice for the thread.<br />
        /// A value less than zero will disable yielding completely.<br />
        /// Values larger than 100 are not allowed.<br />
        /// The default value is 1.
        /// </remarks>
        public static int IdleSleepTime
        {
            get => idleSleepTime;
            set
            {
                if (value > 100) throw new ArgumentOutOfRangeException();
                if (idleSleepTime != value)
                {
                    idleSleepTime = value;
                    RefreshAll();
                }
            }
        }


        /// <summary>
        /// Gets or sets amount of time to sleep for each work repeat, in milliseconds.
        /// </summary>
        /// <remarks>
        /// A value of 0 will yield the remainder of the time slice for the thread.<br />
        /// A value less than zero will disable yielding completely.<br />
        /// The default value is 1.<br />
        /// Values larger than 100 are not allowed.
        /// </remarks>
        public static int WorkIdleSleepTime
        {
            get => workIdleSleepTime;
            set
            {
                if (value > 100) throw new ArgumentOutOfRangeException();

                if (workIdleSleepTime != value)
                {
                    workIdleSleepTime = value;
                    RefreshAll();
                }
            }
        }

        /// <summary>
        /// The number of times to call <see cref="IDistributable.DoWork"/> per cycle.
        /// </summary>
        /// <remarks>
        /// The default is 2.
        /// </remarks>
        public static int WorkRepeat
        {
            get => workRepeat;
            set
            {
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException();

                if (workRepeat != value)
                {
                    workRepeat = value;
                    RefreshAll();
                }
            }
        }

        /// <summary>
        /// Gets or sets the thread priority for distributors.
        /// </summary>
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
                if (distributor == null) return;

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

        /// <summary>
        /// Forces all threads to refresh their current state and recreate all execution delegates.
        /// </summary>
        public static void RefreshAll()
        {
            lock (lockObj)
            {
                foreach (var dist in distributors)
                {
                    dist.ActionsChanged = true;
                }
            }
        }

    }
}
