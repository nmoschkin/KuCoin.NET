using KuCoin.NET.Data.Websockets.User;
using KuCoin.NET.Helpers;

using System;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Private
{
    public class BalanceNoticeFeed : TopicFeedBase<BalanceNotice>
    {

        public event EventHandler<BalanceNoticeEventArgs> BalanceNotification;

        public override bool IsPublic => false;

        public override string Subject => "account.balance";

        public override string Topic => "/account/balance";

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
