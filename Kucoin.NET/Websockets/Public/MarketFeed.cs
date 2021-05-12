using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    /// <summary>
    /// Implements the all symbol market feed (Level 2).
    /// </summary>
    public class MarketFeed : KucoinBaseWebsocketFeed<SnapshotItem>
    {
        private List<string> activeTickers = new List<string>();

        /// <summary>
        /// Instantiate a new all market feed.
        /// </summary>
        public MarketFeed() : base(null, null, null)
        {
        }
        public override bool IsPublic => true;

        protected override string Subject => "trade.snapshot";

        protected override string Topic => "/market/snapshot";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == Subject)
                {
                    var i = msg.Topic.IndexOf(":");
                    // TO DO: Rework data for SnapshotListItem
                    var marketItem = msg.Data["data"].ToObject<SnapshotItem>();

                    if (i != -1)
                    {
                        marketItem.Symbol = msg.Topic.Substring(i + 1);
                    }

                    await PushNext(marketItem);
                }
            }
        }

        /// <summary>
        /// Subscribe to the feed.
        /// </summary>
        /// <param name="market">The market to observe.</param>
        /// <returns></returns>
        public virtual async Task StartFeed(string market)
        {
            if (disposed) throw new ObjectDisposedException(nameof(MarketFeed));
            if (!Connected)
            {
                await Connect();
            }

            var topic = $"{Topic}:{market}";

            var e = new FeedMessage()
            {
                Type = "subscribe",
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
        public override IDisposable Subscribe(IObserver<SnapshotItem> observer)
        {
            lock (observers)
            {
                foreach (var obs in observers)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new GranularObservation<SnapshotItem>(this, observer);
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
        public IDisposable Subscribe(IObserver<SnapshotItem> observer, IEnumerable<string> symbols)
        {
            lock (observers)
            {
                foreach (var obs in observers)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new GranularObservation<SnapshotItem>(symbols, this, observer);
                observers.Add(obsNew);

                return obsNew;
            }
        }

        /// <summary>
        /// Push the object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected override async Task PushNext(SnapshotItem obj)
        {
            await Task.Run(() =>
            {
                foreach (GranularObservation<SnapshotItem> obs in observers)
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
