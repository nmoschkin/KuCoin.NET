using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Market;
using KuCoin.NET.Futures.Websockets.Observations;
using KuCoin.NET.Helpers;
using KuCoin.NET.Json;
using KuCoin.NET.Websockets;
using KuCoin.NET.Websockets.Distribution;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets
{
    public interface IFuturesLevel2 : IMarketFeed<FuturesLevel2OrderBook, FuturesLevel2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>>
    {

    }

    public class FuturesLevel2 : OrderBookFeed<FuturesLevel2OrderBook, FuturesLevel2Update, KeyedOrderBook<OrderUnitStruct>, ObservableOrderBook<ObservableOrderUnit>, FuturesLevel2>, IFuturesLevel2
    {
        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public FuturesLevel2(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (!credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use spot market API credentials on a futures feed.");

            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public FuturesLevel2(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: true)
        {
            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;

        }
        public string Subject => "level2";

        public override string InitialDataUrl => "/api/v1/level2/snapshot";

        public override string Topic => "/contractMarket/level2";

        public override bool IsPublic => false;

        public override void Release(IWebsocketListener obj) => Release((FuturesLevel2OrderBook)obj);

        public void Release(FuturesLevel2OrderBook obj)
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

        protected override FuturesLevel2OrderBook CreateFeed(string sym)
        {
            return new FuturesLevel2OrderBook(this, sym);
        }

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<FuturesLevel2Update>>(json, settings);
            var symbol = msg.Data.Symbol;

            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {
                if (string.IsNullOrEmpty(symbol)) return;
                activeFeeds[symbol].OnNext(msg.Data);
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }

    }
}
