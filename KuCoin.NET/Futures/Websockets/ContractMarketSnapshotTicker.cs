using KuCoin.NET.Futures.Data.Websockets;
using KuCoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Websockets
{
    /// <summary>
    /// Ticker for the 24 hour market snapshot
    /// </summary>
    public class ContractMarketSnapshotTicker : SymbolTopicFeedBase<ContractMarketSnapshot>
    {
        public ContractMarketSnapshotTicker() : base(null, futures: true)
        {

        }

        public override bool IsPublic => true;

        public override string Subject => "snapshot.24h";

        public override string Topic => "/contractMarket/snapshot";
    }
}
