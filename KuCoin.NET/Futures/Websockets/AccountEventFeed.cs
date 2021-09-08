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
    /// Futures account events feed
    /// </summary>
    public class AccountEventFeed : TopicFeedBase<AccountEventData>
    {
        /// <summary>
        /// Order Margin Event
        /// </summary>
        public event EventHandler<AccountEventData> OrderMaginEvent;

        /// <summary>
        /// Balance Changed Event
        /// </summary>
        public event EventHandler<AccountEventData> AvailableBalanceEvent;

        /// <summary>
        /// Withdrawal or Hold Event
        /// </summary>
        public event EventHandler<AccountEventData> WithdrawHoldEvent;

        /// <summary>
        /// Account Event
        /// </summary>
        public event EventHandler<AccountEventData> AccountEvent;

        public AccountEventFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public AccountEventFeed(string key,
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

        public override string Subject => "change";

        public override string Topic => "/contractAccount/wallet";


        protected override async Task HandleMessage(FeedMessage msg)
        {
            
            if (msg.Type == "message" && msg.Topic == Topic)
            {

                var obj = msg.Data.ToObject<AccountEventData>();

                if (msg.Subject == "orderMargin.change")
                {
                    obj.EventType = EventType.OrderMargin;
                }
                else if (msg.Subject == "availableBalance.change")
                {
                    obj.EventType = EventType.OrderMargin;
                }
                else if (msg.Subject == "withdrawHold.change")
                {
                    obj.EventType = EventType.OrderMargin;
                }

                await PushNext(obj);


                AccountEvent?.Invoke(this, obj);

                if (msg.Subject == "orderMargin.change")
                {
                    OrderMaginEvent?.Invoke(this, obj);
                }
                else if (msg.Subject == "availableBalance.change")
                {
                    AvailableBalanceEvent?.Invoke(this, obj);
                }
                else if (msg.Subject == "withdrawHold.change")
                {
                    WithdrawHoldEvent?.Invoke(this, obj);
                }

            }


        }


    }
}
