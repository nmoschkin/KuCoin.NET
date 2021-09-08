using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Futures.Data.Websockets;
using KuCoin.NET.Futures.Websockets.Observations;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Futures.Websockets
{
    /// <summary>
    /// Position Change Events
    /// </summary>
    public class PositionChangeFeed : SymbolTopicFeedBase<PositionData>, IObservable<FundingSettlement>
    {

        public event EventHandler<PositionData> PositionChanged;

        public event EventHandler<FundingSettlement> FundingSettled;

        public PositionChangeFeed(ICredentialsProvider cred) : base(cred, futures: true)
        {
        }

        public PositionChangeFeed(string key,
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

        public override string Subject => "position.change";

        public override string Topic => "/contract/position";


        #region IObservable<T> Pattern

        internal List<FundingObservation> observations2 = new List<FundingObservation>();

        /// <summary>
        /// Subscribe to this feed.
        /// </summary>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        IDisposable IObservable<FundingSettlement>.Subscribe(IObserver<FundingSettlement> observer)
        {
            lock (observations2)
            {
                foreach (var obs in observations2)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new FundingObservation(this, observer);
                observations2.Add(obsNew);

                return obsNew;
            }
        }

        /// <summary>
        /// Fires the <see cref="IObserver{T}.OnCompleted"/> method and removes the observation from the observation list.
        /// The observer will not receive any further notifications.
        /// </summary>
        /// <param name="observation">The observation to remove.</param>
        /// <remarks>
        /// This method is called internally by the various observation classes when the <see cref="IDisposable.Dispose"/> method is called on them.
        /// </remarks>
        internal virtual void RemoveFundingObservation(FundingObservation observation)
        {
            observation.Observer.OnCompleted();

            lock (observations)
            {
                if (observations2.Contains(observation))
                {
                    observations2.Remove(observation);
                }
            }
        }

        /// <summary>
        /// Push the deserialized subscription object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual async Task PushNextFunding(FundingSettlement obj)
        {
            await Task.Run(() =>
            {
                List<Action> actions = new List<Action>();

                foreach (var obs in observations2)
                {
                    actions.Add(() => obs.Observer.OnNext(obj));
                }

                Parallel.Invoke(actions.ToArray());

            });
        }

        #endregion IObservable<T> Pattern

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message" && msg.Subject == "position.settlement")
            {
                var obj = msg.Data.ToObject<FundingSettlement>();
                await PushNextFunding(obj);
                FundingSettled?.Invoke(this, obj);
            }
            else
            {
                await base.HandleMessage(msg);
            }
        }

        protected override async Task PushNext(PositionData obj)
        {
            await base.PushNext(obj);
            PositionChanged?.Invoke(this, obj);
        }
    }
}
