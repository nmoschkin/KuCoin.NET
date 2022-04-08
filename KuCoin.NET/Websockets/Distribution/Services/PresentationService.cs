using KuCoin.NET.Data.User;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static KuCoin.NET.Helpers.GCFHelper;


namespace KuCoin.NET.Websockets.Distribution.Services
{

    /// <summary>
    /// Provides automatic push services for objects that must update UI-facing objects for presentation at regular intervals.
    /// </summary>
    public static class PresentationService
    {
        private static object lockObj = new object();

        private static readonly List<IPresentable> feeds = new List<IPresentable>();

        private static Thread IntervalThread;

        private static CancellationTokenSource cts;

        private static int maxwait;

        /// <summary>
        /// The calculated wait interval.
        /// </summary>
        public static int Wait { get; private set; } = 10;

        private static readonly Dictionary<int, Action[]> triggerGroups = new Dictionary<int, Action[]>();

        private static void UpdateTriggerGroups()
        {
            lock (lockObj)
            {
                triggerGroups.Clear();
                maxwait = 0;

                var actions = new List<Action>();
                Dictionary<int, List<Action>> tmpdict = new Dictionary<int, List<Action>>();

                Action a;
                
                foreach (var feed in feeds)
                {
                    int pi = feed.Interval;

                    if (feed.PreferDispatcher && Dispatcher.Initialized)
                    {
                        a = () => Dispatcher.InvokeOnMainThread((o) => feed.PresentData());
                    }
                    else
                    {
                        a = () => feed.PresentData();
                    }

                    if (!tmpdict.ContainsKey(pi))
                    {
                        tmpdict.Add(feed.Interval, new List<Action>() { a });
                    }
                    else
                    {
                        tmpdict[pi].Add(a);
                    }

                    if (pi > maxwait) maxwait = pi;
                }

                if (tmpdict.Count != 0)
                {
                    if (tmpdict.Count > 1)
                    {
                        Wait = FindGCF(tmpdict.Keys.ToArray());
                    }
                    else
                    {
                        Wait = tmpdict.Keys.First();
                    }

                    foreach (var kv in tmpdict)
                    {
                        triggerGroups.Add(kv.Key, kv.Value.ToArray());
                    }
                }
            }
        }

        static PresentationService()
        {
        }

        /// <summary>
        /// Register a new presentable object.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <returns>True if the object was successfully registered, false if the object was already registered.</returns>
        public static bool RegisterService(IPresentable obj)
        {
            lock (lockObj)
            {
                if (!feeds.Contains(obj))
                {
                    obj.PropertyChanged += OnFeedPropertyChanged;

                    feeds.Add(obj);
                }
                else
                {
                    return false;
                }

                UpdateTriggerGroups();

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

        private static void OnFeedPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPresentable.Interval))
            {
                UpdateTriggerGroups();
            }
        }

        /// <summary>
        /// Unregister a presentable object.
        /// </summary>
        /// <param name="obj">The object to unregister.</param>
        /// <returns>True if the object was successfully unregistered, false if the object was not registered.</returns>
        public static bool UnregisterService(IPresentable obj)
        {
            lock (lockObj)
            {
                if (feeds.Contains(obj))
                {
                    feeds.Remove(obj);
                }
                else
                {
                    return false;
                }

                if (feeds.Count == 0)
                {
                    CancelIntervalThread();
                }
                else
                {
                    UpdateTriggerGroups();
                }
                return true;
            }
        }

        /// <summary>
        /// Cancel the observer thread.
        /// </summary>
        private static void CancelIntervalThread()
        {
            cts?.Cancel();

            IntervalThread = null;
            triggerGroups.Clear();
            cts = null;
        }

        /// <summary>
        /// Observer thread.
        /// </summary>
        private static void IntervalMethod()
        {
            int wait = Wait;

            while (!cts?.IsCancellationRequested ?? false)
            {
                try
                {
                    for (int i = 0; i <= maxwait; i += wait)
                    {
                        Task.Delay(wait, cts?.Token ?? default).ConfigureAwait(false).GetAwaiter().GetResult();

                        lock (lockObj)
                        {
                            if (triggerGroups.TryGetValue(i, out Action[] execs))
                            {
                                Parallel.Invoke(execs);
                            }
                            if (wait != Wait)
                            {
                                wait = Wait;
                                break;
                            }
                        }
                    }

                    if (cts?.IsCancellationRequested ?? true) return;
                }
                catch //(Exception ex)
                {

                }

            }
        }


    }
}
