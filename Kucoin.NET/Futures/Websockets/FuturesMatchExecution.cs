using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Websockets
{
    public class FuturesMatchExecution : SymbolTopicFeedBase<FuturesMatch>
    {
        public FuturesMatchExecution() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        protected override string Subject => "match";

        protected override string Topic => "/contractMarket/execution";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message" && msg.Subject == Subject)
            {
                if (msg.Topic.Contains(topic))
                {
                    var i = msg.Topic.IndexOf(":");
                    var symbol = msg.Data.ToObject<FuturesMatch>();

                    if (i != -1)
                    {
                        symbol.Symbol = msg.Topic.Substring(i + 1);
                    }
                
                    await PushNext(symbol);
                }
            }
        }
    }
}
