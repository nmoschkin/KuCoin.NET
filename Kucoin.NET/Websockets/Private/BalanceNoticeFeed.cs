using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Websockets.User;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Private
{
    public class BalanceNoticeFeed : KucoinBaseWebsocketFeed<BalanceNotice>
    {

        public event EventHandler<BalanceNoticeEventArgs> BalanceNotification;

        public override bool IsPublic => false;

        protected override string Subject => "account.balance";

        protected override string Topic => "/account/balance";

        public BalanceNoticeFeed(ICredentialsProvider credProvider, bool isSandbox = false) : base(credProvider, isSandbox)
        {
        }

        public BalanceNoticeFeed(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Subject == Subject)
            {
                var obj = msg.Data.ToObject<BalanceNotice>();
                await PushNext(obj);
            }
        }

        protected override async Task PushNext(BalanceNotice obj)
        {
            var bn = obj.Clone();

            await base.PushNext(obj);
            BalanceNotification?.Invoke(this, new BalanceNoticeEventArgs(bn));
        }

    }
}
