using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets.Public
{

    public class SymbolKline
    {
        KlineType type;

        string symbol;

        public KlineType KlineType
        {
            get => type;
            set => type = value;
        }

        public string Symbol
        {
            get => symbol;
            set => symbol = value;
        }

        public SymbolKline(string symbol, KlineType type)
        {
            this.type = type;
            this.symbol = symbol;
        }
        public SymbolKline()
        {

        }

        public override string ToString()
        {
            return $"{symbol}_{type}";
        }

        public static SymbolKline Parse(string s)
        {
            var parts = s.Split('_');

            var output = new SymbolKline();

            output.Symbol = parts[0];
            output.KlineType = KlineType.Parse(parts[1]);

            return output;
        }

    }


    /// <summary>
    /// Implements the symbol candles feed (Level 2).
    /// </summary>
    public class KlineFeed<T> : KucoinBaseWebsocketFeed<KlineFeedMessage<T>> where T: IWriteableCandle, new()
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
                await AddSymbol(symbol, type);
            });
        }

        public override bool IsPublic => true;

        protected override string Subject => "trade.candles.update";

        protected override string Topic => "/market/candles";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == Subject)
                {
                    var i = msg.Topic.IndexOf(":");
                    if (i == -1) return;

                    var ticker = new KlineFeedMessage<T>();
                    
                    var sk = SymbolKline.Parse(msg.Topic.Substring(i + 1));

                    ticker.Time = msg.Data["time"].ToObject<long>();
                    ticker.Symbol = msg.Data["symbol"].ToObject<string>();

                    var values = msg.Data["candles"].ToObject<string[]>();

                    var candle = new T();

                    candle.Timestamp = EpochTime.SecondsToDate(long.Parse(values[0]));

                    candle.OpenPrice = decimal.Parse(values[1]);
                    candle.ClosePrice = decimal.Parse(values[2]);

                    candle.HighPrice = decimal.Parse(values[3]);
                    candle.LowPrice = decimal.Parse(values[4]);

                    candle.Amount = decimal.Parse(values[5]);
                    candle.Volume = decimal.Parse(values[6]);

                    if (candle is IWriteableTypedCandle wt)
                    {
                        wt.Type = sk.KlineType;
                    }

                    ticker.Candles = candle;

                    await PushNext(ticker);
                }
            }
        }

        /// <summary>
        /// Add to the specified ticker
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task AddSymbol(string symbol, KlineType type)
        {
            if (disposed) throw new ObjectDisposedException(nameof(KlineFeed<T>));
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
                Response = true
            };

            await Send(e);

        }

        /// <summary>
        /// Remove from the specified ticker
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public virtual async Task RemoveSymbol(string symbol, KlineType type)
        {
            if (disposed) throw new ObjectDisposedException(nameof(KlineFeed<T>));
            if (!Connected) return;

            SymbolKline sk = null;

            foreach (var t in activeTickers)
            {
                if (t.Symbol == symbol && t.KlineType == type)
                {
                    sk = t;
                    break;
                }
            }

            if (sk == null) return;

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
        /// Remove from all tickers
        /// </summary>
        /// <returns></returns>
        public virtual async Task RemoveAllTickers()
        {
            foreach (var s in activeTickers)
            {
                await RemoveSymbol(s.Symbol, s.KlineType);
            }

        }

    }
}
