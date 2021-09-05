using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Distribution;
using Kucoin.NET.Websockets.Observations;
using System.Threading;
using Kucoin.NET.Json;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;

namespace Kucoin.NET.Websockets.Public
{
    public interface ILevel3 : IMarketFeed<Level3OrderBook, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>
    {

    }

    public class Level3 : OrderBookFeed<Level3OrderBook, Level3Update, KeyedAtomicOrderBook<AtomicOrderStruct>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3>, ILevel3
    {
        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level3(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;

            settings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[]
                {
                    new Level3UpdateConverter()
                }
            };
        }

        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level3(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: false)
        {
            recvBufferSize = 4194304;
            minQueueBuffer = 10000;
            chunkSize = 1024;

            settings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[]
                {
                    new Level3UpdateConverter()
                }
            };
        }

        public override string Topic => "/spotMarket/level3";

        public override string InitialDataUrl => "/api/v3/market/orderbook/level3";

        public override bool IsPublic => false;

        public override void Release(IWebsocketListener obj) => Release((Level3OrderBook)obj);

        public void Release(Level3OrderBook obj)
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

        protected override Level3OrderBook CreateFeed(string sym)
        {
            return new Level3OrderBook(this, sym);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            JsonConvert.PopulateObject(json, msg, settings);

            if (msg.Type[0] == 'm' && msg.TunnelId == tunnelId)
            {
                var update = msg.Data;
                update.Subject = msg.Subject;

                activeFeeds[update.Symbol].OnNext(update);
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }
    }
}
