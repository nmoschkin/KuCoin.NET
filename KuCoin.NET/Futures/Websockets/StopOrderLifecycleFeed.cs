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
    /// Stop Order Lifecycle Event Feed
    /// </summary>
    public class StopOrderLifecycleFeed : TopicFeedBase<AdvancedOrder>
    {


        public StopOrderLifecycleFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public StopOrderLifecycleFeed(string key,
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

        public override string Subject => "stopOrder";

        public override string Topic => "/contractMarket/advancedOrders";
    }
}
