using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets
{
    /// <summary>
    /// Trade Order Feed
    /// </summary>
    public class TradeOrderFeed : TopicFeedBase<TradeOrder>
    {
        public TradeOrderFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public TradeOrderFeed(string key,
            string secret,
            string passphrase,
            bool isSandbox = false)
            : base(
                  key,
                  secret,
                  passphrase,
                  isSandbox,
                  futures: true)
        {
        }

        public override bool IsPublic => false;

        public override string Subject => "orderChange";

        public override string Topic => "/contractMarket/tradeOrders";

    }

    /// <summary>
    /// Symbol-based Trade Order Feed
    /// </summary>
    public class SymbolTradeOrderFeed : SymbolTopicFeedBase<TradeOrder>
    {

        public SymbolTradeOrderFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public SymbolTradeOrderFeed(string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                  key, 
                  secret, 
                  passphrase, 
                  isSandbox, 
                  true)
        {
        }

        public override bool IsPublic => false;

        public override string Subject => "symbolOrderChange";

        public override string Topic => "/contractMarket/tradeOrders";
    }
}
