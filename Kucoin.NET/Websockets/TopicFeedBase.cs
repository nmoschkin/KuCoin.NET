using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets
{
    public abstract class TopicFeedBase<T> : KucoinBaseWebsocketFeed<T> where T: class
    {
        protected bool feedStarted = false;

        protected object lockObj = new object();

        protected bool autoStartStop = true;

        public TopicFeedBase(
        string key,
        string secret,
        string passphrase,
        bool isSandbox = false)
        : base(key, secret, passphrase, isSandbox)
        {
        }

        public TopicFeedBase(ICredentialsProvider credProvider) : base(credProvider)
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

        public virtual bool AutoStartStop
        {
            get => autoStartStop;
            set 
            {
                SetProperty(ref autoStartStop, value);
            }
        }

        public virtual bool FeedStarted
        {
            get => feedStarted;
            protected set
            {
                SetProperty(ref feedStarted, value);
            }
        }

        public virtual async Task StartFeed()
        {
            lock(lockObj)
            {
                if (feedStarted) return;
                feedStarted = true;
            }

            if (disposed) throw new ObjectDisposedException(nameof(TopicFeedBase<T>));
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

        public virtual async Task StopFeed()
        {
            lock(lockObj)
            {
                if (!feedStarted) return;
                feedStarted = false;
            }

            if (disposed) throw new ObjectDisposedException(nameof(TopicFeedBase<T>));
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

        internal override void RemoveObservation(FeedObservation<T> observation)
        {
            base.RemoveObservation(observation);
            if (autoStartStop && observers.Count == 0)
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
