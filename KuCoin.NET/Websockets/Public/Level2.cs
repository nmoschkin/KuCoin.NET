using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Observations;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Public
{
    public interface ILevel2 : IMarketFeed<Level2OrderBook, Level2Update, AggregatedOrderBook<OrderUnit>, ObservableOrderBook<ObservableOrderUnit>>
    {

    }

    public class Level2 : OrderBookFeed<Level2OrderBook, Level2Update, AggregatedOrderBook<OrderUnit>, ObservableOrderBook<ObservableOrderUnit>, Level2>, ILevel2, IInitialDataProviderCallback<string, AggregatedOrderBook<OrderUnit>>
    {
        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level2(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

            recvBufferSize = 8 * 1024 * 1024;
            minQueueBuffer = 1000;
            chunkSize = 1024;
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: false)
        {
            recvBufferSize = 8 * 1024 * 1024;
            minQueueBuffer = 1000;
            chunkSize = 1024;

        }
        public override string InitialDataUrl => "/api/v3/market/orderbook/level2";

        public string Subject => "trade.l2update";

        public override string Topic => "/market/level2";

        public override bool IsPublic => false;


        public override void Release(IWebsocketListener obj) => Release((Level2OrderBook)obj);

        public void Release(Level2OrderBook obj)
        {
            if (activeFeeds.ContainsValue(obj))
            {
                try
                {
                    UnsubscribeOne(obj.Key).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {

                }
            }
        }

        protected override Level2OrderBook CreateFeed(string sym)
        {
            return new Level2OrderBook(this, sym);
        }

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<Level2Update>>(json, settings);
            string symbol = msg.Data?.Symbol;

            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {
                if (string.IsNullOrEmpty(symbol)) return;
                msg.Data.size = json.Length;
                activeFeeds[symbol].OnNext(msg.Data);
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }

    }
}
