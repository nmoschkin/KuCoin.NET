using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    /// <summary>
    /// Implements the all symbol ticker feed (Level 2).
    /// </summary>
    public class AllTickerFeed : KucoinBaseWebsocketFeed<Ticker>
    {
        private List<string> activeTickers = new List<string>();

        /// <summary>
        /// Instantiate a new all ticker feed.
        /// </summary>
        public AllTickerFeed() : base(null, null, null)
        {
        }
        public override bool IsPublic => true;

        protected override string Subject => "trade.ticker";

        protected override string Topic => "/market/ticker:all";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Topic == Topic)
                {
                    var ticker = msg.Data.ToObject<Ticker>();
                    ticker.Symbol = msg.Subject;

                    await PushNext(ticker);
                }
            }
        }

        /// <summary>
        /// Subscribe to the feed.
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public virtual async Task StartFeed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(AllTickerFeed));
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
        /// Unsubscribe from the feed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StopFeed()
        {

            if (disposed) throw new ObjectDisposedException(nameof(AllTickerFeed));
            if (!Connected)
            {
                await Connect();
            }

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

        #region IObservable<T> Pattern

        /// <summary>
        /// Subscribe to this feed.
        /// </summary>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public override IDisposable Subscribe(IObserver<Ticker> observer)
        {
            lock (observers)
            {
                foreach (var obs in observers)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new GranularObservation<Ticker>(this, observer);
                observers.Add(obsNew);

                return obsNew;
            }

        }

        /// <summary>
        /// Subscribe to this feed for only the specified tickers.
        /// </summary>
        /// <param name="symbols">The list of symbols that the observer will observe.</param>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public IDisposable Subscribe(IObserver<Ticker> observer, IEnumerable<string> symbols)
        {
            lock (observers)
            {
                foreach (var obs in observers)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new GranularObservation<Ticker>(symbols, this, observer);
                observers.Add(obsNew);

                return obsNew;
            }
        }

        /// <summary>
        /// Push the object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected override async Task PushNext(Ticker obj)
        {
            await Task.Run(() =>
            {
                foreach (GranularObservation<Ticker> obs in observers)
                {
                    if (obs.ActiveSymbols.Count == 0 || obs.ActiveSymbols.Contains(obj.Symbol))
                    {
                        obs.Observer.OnNext(obj);
                    }
                }
            });
        }

        #endregion

    }
}
