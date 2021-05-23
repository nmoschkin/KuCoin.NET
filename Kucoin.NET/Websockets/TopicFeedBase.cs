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
        protected bool feedStarted;

        public TopicFeedBase(
        string key,
        string secret,
        string passphrase,
        bool isSandbox = false)
        : base(key, secret, passphrase, isSandbox)
        {
        }

        public TopicFeedBase(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
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
            if (observers.Count == 0)
            {
                _ = StopFeed();
            }
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            return base.Subscribe(observer);
        }



    }
}
