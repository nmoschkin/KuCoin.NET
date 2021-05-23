using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Websockets.User;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Private
{

    /// <summary>
    /// Monitors the private order changed events feed.
    /// </summary>
    public class OrderChangeEventsFeed : TopicFeedBase<OrderChange>
    {
        public override bool IsPublic => false;

        protected override string Subject => "orderChange";

        protected override string Topic => "/spotMarket/tradeOrders";

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


        public OrderChangeEventsFeed(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
        }

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
