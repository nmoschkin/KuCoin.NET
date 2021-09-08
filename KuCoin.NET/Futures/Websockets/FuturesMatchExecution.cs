using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Websockets;
using KuCoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets
{
    /// <summary>
    /// Futures Match Execution Event Feed
    /// </summary>
    public class FuturesMatchExecution : SymbolTopicFeedBase<FuturesMatch>
    {
        public FuturesMatchExecution() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        public override string Subject => "match";

        public override string Topic => "/contractMarket/execution";

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
