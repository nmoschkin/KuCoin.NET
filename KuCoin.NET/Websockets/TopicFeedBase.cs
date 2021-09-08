using KuCoin.NET.Data;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Base class for simple topic feeds.
    /// </summary>
    /// <typeparam name="T">The type of object that is pushed on the feed.</typeparam>
    public abstract class TopicFeedBase<T> : KucoinBaseWebsocketFeed<T> where T : class, IStreamableObject
    {
        protected bool feedStarted = false;

        protected object lockObj = new object();

        protected bool autoStartStop = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public TopicFeedBase(
                string key,
                string secret,
                string passphrase,
                bool isSandbox = false,
                bool futures = false)
        : base(key, secret, passphrase, isSandbox, futures: futures)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public TopicFeedBase(ICredentialsProvider credProvider, bool futures = false) : base(credProvider, futures: futures)
        {
        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == Subject)
                {
                    var obj = msg.Data.ToObject<T>();
                    await PushNext(obj);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically start the feed with the addition of the first subscription and stop the feed with the removal of the last subscription.
        /// </summary>
        public virtual bool AutoStartStop
        {
            get => autoStartStop;
            set 
            {
                SetProperty(ref autoStartStop, value);
            }
        }

        /// <summary>
        /// True if the feed is running.
        /// </summary>
        public virtual bool FeedStarted
        {
            get => feedStarted;
            protected set
            {
                SetProperty(ref feedStarted, value);
            }
        }

        /// <summary>
        /// Subscribe to the remote topic and start the feed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StartFeed()
        {
            lock(lockObj)
            {
                if (feedStarted) return;
                feedStarted = true;
            }

            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var topic = Topic;

            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true
            };

            await Send(e);
        }

        /// <summary>
        /// Unsubscribe from the remote topic and stop the feed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StopFeed()
        {
            lock(lockObj)
            {
                if (!feedStarted) return;
                feedStarted = false;
            }

            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            var topic = Topic;

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true
            };

            await Send(e);
        }

        internal override void RemoveObservation(FeedObject<T> observation)
        {
            base.RemoveObservation(observation);
            if (autoStartStop && observations.Count == 0)
            {
                StopFeed().Wait();
            }
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            if (autoStartStop && !feedStarted)
            {
                StartFeed().Wait();
            }
        
            return base.Subscribe(observer);
        }

    }
}
