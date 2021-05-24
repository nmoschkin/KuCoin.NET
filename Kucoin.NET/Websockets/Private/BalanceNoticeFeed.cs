using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Websockets.User;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Private
{
    public class BalanceNoticeFeed : TopicFeedBase<BalanceNotice>
    {

        public event EventHandler<BalanceNoticeEventArgs> BalanceNotification;

        public override bool IsPublic => false;

        protected override string Subject => "account.balance";

        protected override string Topic => "/account/balance";

        public BalanceNoticeFeed(ICredentialsProvider credProvider) : base(credProvider)
        {
        }

        public BalanceNoticeFeed(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox)
        {
        }

        protected override async Task PushNext(BalanceNotice obj)
        {
            var bn = obj.Clone();

            _ = Task.Run(() => BalanceNotification?.Invoke(this, new BalanceNoticeEventArgs(bn)));
            await base.PushNext(obj);
        }

    }
}
