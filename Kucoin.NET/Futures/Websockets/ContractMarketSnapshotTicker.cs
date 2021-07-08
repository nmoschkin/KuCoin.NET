using Kucoin.NET.Futures.Data.Websockets;
using Kucoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Websockets
{
    public class ContractMarketSnapshotTicker : SymbolTopicFeedBase<ContractMarketSnapshot>
    {
        public ContractMarketSnapshotTicker() : base(null, futures: true)
        {

        }

        public override bool IsPublic => true;

        protected override string Subject => "snapshot.24h";

        protected override string Topic => "/contractMarket/snapshot";
    }
}
