using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KuCoin.NET.Helpers;

namespace KuCoin.NET.Websockets.Public
{

    /// <summary>
    /// Implements the symbol candles feed (Level 2).
    /// </summary>
    /// <typeparam name="TCandle">The type of the <see cref="IFullCandle"/> interface implementation to serve.</typeparam>
    public class KlineFeed<TCandle> : KucoinBaseWebsocketFeed<KlineFeedMessage<TCandle>> where TCandle: IFullCandle, new()
    {

        private List<SymbolKline> activeTickers = new List<SymbolKline>();

        /// <summary>
        /// Instantiate a new symbol ticker feed.
        /// </summary>
        public KlineFeed() : base(null, null, null)
        {
        }

        /// <summary>
        /// Instantiate and connect a new Level 2 data feed.
        /// </summary>
        /// <param name="symbol">The symbol to watch.</param>
        public KlineFeed(string symbol, KlineType type) : base(null, null, null)
        {
            Connect().ContinueWith(async (t) =>
            {
                await SubscribeOne(symbol, type);
            });
        }

        public override bool IsPublic => true;

        public override string Subject => "trade.candles.update";

        public override string Topic => "/market/candles";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == Subject)
                {
                    var i = msg.Topic.IndexOf(":");
                    if (i == -1) return;

                    var ticker = new KlineFeedMessage<TCandle>();
                    
                    var sk = SymbolKline.Parse(msg.Topic.Substring(i + 1));

                    // The JSON.NET documentation states clearly that
                    // for tricky deserialization scenarios, the fastest
                    // way is manual deserialization.

                    ticker.Timestamp = EpochTime.NanosecondsToDate(msg.Data["time"].ToObject<long>());
                    ticker.Symbol = msg.Data["symbol"].ToObject<string>();

                    // Here we have the candle data efficiently served as an array of strings.
                    // In C# we want to parse this array into a strongly-typed object.

                    var values = msg.Data["candles"].ToObject<string[]>();
                    var candle = new TCandle
                    {
                        Timestamp = EpochTime.SecondsToDate(long.Parse(values[0])),

                        // We are going to assume that the data from the feed is correctly formatted.
                        // If an error is thrown here, then there is a deeper problem.

                        OpenPrice = decimal.Parse(values[1]),
                        ClosePrice = decimal.Parse(values[2]),

                        HighPrice = decimal.Parse(values[3]),
                        LowPrice = decimal.Parse(values[4]),

                        Amount = decimal.Parse(values[5]),
                        Volume = decimal.Parse(values[6])
                    };

                    // The candlestick does not need to be a typed candle,
                    // but if it is, we will set that value, as well.
                    if (candle is IFullKlineCandle<KlineType> wt)
                    {
                        wt.Type = sk.KlineType;
                    }

                    ticker.Candles = candle;

                    // push the final object.
                    await PushNext(ticker);
                }
            }
        }

        /// <summary>
        /// Subscribe to the specified ticker.
        /// </summary>
        /// <param name="symbolKline">The <see cref="SymbolKline"/> combination of the ticker to add.</param>
        /// <returns></returns>
        public virtual async Task SubscribeOne(SymbolKline symbolKline)
        {
            await SubscribeOne(symbolKline.Symbol, symbolKline.KlineType);
        }

        /// <summary>
        /// Subscribe to the specified ticker.
        /// </summary>
        /// <param name="symbol">The trading pair (symbol) of the ticker to add.</param>
        /// <param name="type">The <see cref="KlineType"/> of the ticker to add.</param>
        /// <returns></returns>
        public virtual async Task SubscribeOne(string symbol, KlineType type)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            foreach (var t in activeTickers)
            {
                if (t.Symbol == symbol && t.KlineType == type) return;
            }

            var sk = new SymbolKline(symbol, type);

            activeTickers.Add(sk);

            var topic = $"{Topic}:{sk}";


            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

        }


        /// <summary>
        /// Unsubscribe from the specified ticker.
        /// </summary>
        /// <param name="symbolKline">The <see cref="SymbolKline"/> combination of the ticker to remove.</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeOne(SymbolKline symbolKline)
        {
            await UnsubscribeOne(symbolKline.Symbol, symbolKline.KlineType);
        }

        /// <summary>
        /// Unsubscribe from the specified ticker.
        /// </summary>
        /// <param name="symbol">The trading pair (symbol) of the ticker to remove.</param>
        /// <param name="type">The <see cref="KlineType"/> of the ticker to remove.</param>
        public virtual async Task UnsubscribeOne(string symbol, KlineType type)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            SymbolKline sk = SymbolKline.Empty;

            foreach (var t in activeTickers)
            {
                if (t.Symbol == symbol && t.KlineType == type)
                {
                    sk = t;
                    break;
                }
            }

            if (sk == SymbolKline.Empty) return;

            var topic = $"{Topic}:{sk}";


            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true
            };

            await Send(e);
        }

        /// <summary>
        /// Unsubscribe from all tickers.
        /// </summary>
        /// <returns></returns>
        public virtual async Task ClearAllTickers()
        {
            foreach (var s in activeTickers)
            {
                await UnsubscribeOne(s.Symbol, s.KlineType);
            }

        }

        /// <summary>
        /// Gets a list of all the active tickers subscribed for this kline feed.
        /// </summary>
        public IReadOnlyList<SymbolKline> ActiveTickers => activeTickers;

    }
}
