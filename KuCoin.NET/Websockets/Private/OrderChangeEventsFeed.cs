using KuCoin.NET.Data.Websockets.User;
using KuCoin.NET.Helpers;

using System;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Private
{

    /// <summary>
    /// Monitors the private order changed events feed.
    /// </summary>
    public class OrderChangeEventsFeed : TopicFeedBase<OrderChange>
    {
        public override bool IsPublic => false;

        public override string Subject => "orderChange";

        public override string Topic => "/spotMarket/tradeOrders";

        /// <summary>
        /// When the order enters into the order book
        /// </summary>
        public event EventHandler<OrderChangeEventArgs> Open;

        /// <summary>
        /// When the order has been executed
        /// </summary>
        public event EventHandler<OrderChangeEventArgs> Match;


        /// <summary>
        /// When the order has been executed and its status was changed into DONE
        /// </summary>
        public event EventHandler<OrderChangeEventArgs> Filled;


        /// <summary>
        /// When the order has been cancelled and its status was changed into DONE
        /// </summary>
        public event EventHandler<OrderChangeEventArgs> Canceled;


        /// <summary>
        /// When the order has been updated
        /// </summary>
        public event EventHandler<OrderChangeEventArgs> Update;

        /// <summary>
        /// Create a new feed to follow order change events with the specified credentials.
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        public OrderChangeEventsFeed(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        /// <summary>
        /// Create a new feed to follow order change events with the specified credentials.
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        public OrderChangeEventsFeed(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

        protected override async Task PushNext(OrderChange obj)
        {            
            var ch = obj.Clone();
            await base.PushNext(obj);
            
            var e = new OrderChangeEventArgs(ch);

            switch (ch.Type)
            {
                case OrderEventType.Canceled:
                    Canceled?.Invoke(this, e);
                    break;

                case OrderEventType.Filled:
                    Filled?.Invoke(this, e);
                    break;

                case OrderEventType.Match:
                    Match?.Invoke(this, e);
                    break;

                case OrderEventType.Open:
                    Open?.Invoke(this, e);
                    break;

                case OrderEventType.Update:
                    Update?.Invoke(this, e);
                    break;
            }
        }
     
    }
}
