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
    /// Ticker to watch the contract market
    /// </summary>
    public class ContractMarketTicker : SymbolTopicFeedBase<ContractMarketData>
    {

        public ContractMarketTicker() : base(null, futures: true)
        {
        }

        public override bool IsPublic => true;

        public override string Subject => "mark.index.price";

        public override string Topic => "/contract/instrument";

        protected override async Task HandleMessage(FeedMessage msg)
        {
            await base.HandleMessage(msg);
            if (msg.Type == "message" && (msg.Topic?.Contains(Topic) ?? false))
            {
                var i = msg.Topic.IndexOf(":");
                var symbol = msg.Data.ToObject<ContractMarketData>();

                if (i != -1)
                {
                    symbol.Symbol = msg.Topic.Substring(i + 1);
                }

                if (msg.Subject == "mark.index.price")
                {
                    symbol.Type = ContractDataType.MarkIndexPrice;
                }
                else if (msg.Subject == "funding.rate")
                {
                    symbol.Type = ContractDataType.FundingRate;
                }

                await PushNext(symbol);
            }
        }

    }
}
