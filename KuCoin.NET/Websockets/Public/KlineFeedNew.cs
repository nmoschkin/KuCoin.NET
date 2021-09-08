using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Rest;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Observations;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Public
{
    public class KlineFeedNew<TCandle, TCustom, TCol> : DistributionFeed<KlineChart<TCandle, TCustom, TCol>, SymbolKline, KlineFeedMessage<TCandle>, TCol>
        where TCandle : IFullCandle, TCustom, new()
        where TCol : IList<TCustom>, new()
    {

        public KlineFeedNew() : base(null)
        {
        }


        public override bool IsPublic => true;

        public string Subject => "trade.candles.update";

        public override string Topic => "/market/candles";

        public override string InitialDataUrl => "/api/v1/market/candles";


        public override async Task<TCol> ProvideInitialData(SymbolKline key)
        {
            return await Market.Instance.GetKline<TCandle, TCustom, TCol>(key.Symbol, key.KlineType);
        }

        public override void Release(IWebsocketListener obj)
        {
            obj.Dispose();

            if (obj is KlineChart<TCandle, TCustom, TCol> chart)
            {
                activeFeeds.Remove(chart.Key);
            }
        }

        public override async Task<KlineChart<TCandle, TCustom, TCol>> SubscribeOne(SymbolKline key)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            foreach (var t in activeFeeds.Keys)
            {
                if (t.Symbol == key.Symbol && t.KlineType == key.KlineType) return null;
            }

            var sk = key;
            var newFeed = new KlineChart<TCandle, TCustom, TCol>(this, sk);
            activeFeeds.Add(sk, newFeed);

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

            return newFeed;
        }
        public override async Task UnsubscribeOne(SymbolKline key)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            SymbolKline sk = SymbolKline.Empty;

            foreach (var t in activeFeeds.Keys)
            {
                if (t.Symbol == key.Symbol && t.KlineType == key.KlineType)
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

        public override async Task<IDictionary<SymbolKline, KlineChart<TCandle, TCustom, TCol>>> SubscribeMany(IEnumerable<SymbolKline> keys)
        {
            var d = new Dictionary<SymbolKline, KlineChart<TCandle, TCustom, TCol>>();

            foreach (var key in keys)
            {
                var result = await SubscribeOne(key);
                if (result != null)
                {
                    d.Add(key, result);
                }
            }

            return d;
        }

        public override async Task UnsubscribeMany(IEnumerable<SymbolKline> keys)
        {
            foreach (var key in keys)
            {
                await UnsubscribeOne(key);
            }
        }


        protected override async Task HandleMessage(FeedMessage msg)
        {
            await Task.Run(() =>
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

                        activeFeeds[sk].OnNext(ticker);
                    }
                }

            });
        }
    }
}
