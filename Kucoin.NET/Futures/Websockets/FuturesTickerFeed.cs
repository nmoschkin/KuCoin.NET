using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Market;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public class FuturesTickerFeed : SymbolTopicFeedBase<FuturesTicker>
    {
        public FuturesTickerFeed() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        protected override string Subject => "tickerV2";

        protected override string Topic => "/contractMarket/tickerV2";

        protected override Task HandleMessage(FeedMessage msg)
        {
            return base.HandleMessage(msg);
        }

    }
}
