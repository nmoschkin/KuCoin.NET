using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KuCoin.NET.Helpers;
using KuCoin.NET.Data;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Base class for live feeds supporting multiple symbols per feed.
    /// </summary>
    public abstract class SymbolTopicFeedBase<T> : KucoinBaseWebsocketFeed<T> where T: class, ISymbol, IStreamableObject
    {
        protected List<string> activeSymbols = new List<string>();

        protected string subject;
        
        protected string topic;

        public override event EventHandler<FeedDataReceivedEventArgs<T>> FeedDataReceived;
        public override bool IsPublic => false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public SymbolTopicFeedBase(
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false,
          bool futures = false)
          : base(key, secret, passphrase, isSandbox, futures: futures)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public SymbolTopicFeedBase(ICredentialsProvider credProvider, bool futures = false) : base(credProvider, futures: futures)
        {
        }

        /// <summary>
        /// Returns a list of all the active symbols.
        /// </summary>
        public IReadOnlyList<string> ActiveSymbols => activeSymbols.ToArray();

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (msg.Subject == subject)
                {
                    var i = msg.Topic.IndexOf(":");
                    var symbol = msg.Data.ToObject<T>();

                    if (i != -1)
                    {
                        symbol.Symbol = msg.Topic.Substring(i + 1);
                    }
                    await PushNext(symbol);
                }
            }
        }

        
        /// <summary>
        /// Add to the specified symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task SubscribeOne(string symbol)
        {
            await SubscribeMany(new string[] { symbol });
        }

        /// <summary>
        /// Add to the specified symbols
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public virtual async Task SubscribeMany(IEnumerable<string> symbols)
        {
            if (subject == null) subject = Subject;
            if (this.topic == null) this.topic = Topic;

            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (activeSymbols.Contains(sym)) continue;

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                activeSymbols.Add(sym);
            }
            
            if (sb.Length == 0) return;

            var topic = $"{this.topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = !IsPublic
            };

            await Send(e);

        }

        /// <summary>
        /// Remove from the specified symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task UnsubscribeOne(string symbol)
        {
            await UnsubscribeMany(new string[] { symbol });
        }

        /// <summary>
        /// Remove from the specified symbols
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public virtual async Task UnsubscribeMany(IEnumerable<string> symbols)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (!activeSymbols.Contains(sym)) continue;

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                activeSymbols.Remove(sym);
            }

            if (sb.Length == 0) return;

            var topic = $"{this.topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true, 
                PrivateChannel = !IsPublic
            };

            await Send(e);
        }

        /// <summary>
        /// Remove from all symbols
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAll()
        {
            await UnsubscribeMany(new List<string>(activeSymbols));
        }

        #region IObservable<T> Pattern

        /// <summary>
        /// Subscribe to this feed.
        /// </summary>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public override IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observations)
            {
                foreach (var obs in observations)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new SymbolObservation<T>(this, observer);
                observations.Add(obsNew);

                return obsNew;
            }

        }

        /// <summary>
        /// Subscribe to this feed for only the specified symbols.
        /// </summary>
        /// <param name="symbols">The list of symbols that the observer will observe.</param>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public virtual IDisposable Subscribe(IObserver<T> observer, IEnumerable<string> symbols)
        {
            lock (observations)
            {
                foreach (var obs in observations)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new SymbolObservation<T>(symbols, this, observer);
                observations.Add(obsNew);

                return obsNew;
            }
        }

        /// <summary>
        /// Push the object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected override async Task PushNext(T obj)
        {

            await Task.Run(() =>
            {
                //List<Action> actions = new List<Action>();

                //if (observations != null && observations.Count != 0)
                //{
                //    foreach (SymbolObservation<T> obs in observations)
                //    {
                //        if (obs.ActiveSymbols.Count == 0 || obs.ActiveSymbols.Contains(obj.Symbol))
                //        {
                //            actions.Add(() => obs.Observer.OnNext(obj));
                //        }
                //    }
                //}

                //if (FeedDataReceived != null)
                //{
                //    actions.Add(() =>
                //    {
                //        FeedDataReceived.Invoke(this, new FeedDataReceivedEventArgs<T>(obj));
                //    });
                //}

                //if (actions.Count > 0) Parallel.Invoke(actions.ToArray());

                if (observations != null && observations.Count != 0)
                {
                    foreach (SymbolObservation<T> obs in observations)
                    {
                        if (obs.ActiveSymbols.Count == 0 || obs.ActiveSymbols.Contains(obj.Symbol))
                        {
                            obs.Observer.OnNext(obj);
                        }
                    }
                }

                if (FeedDataReceived != null)
                {
                    FeedDataReceived.Invoke(this, new FeedDataReceivedEventArgs<T>(obj));
                }

            });
        }

        #endregion

        #region Multiplexing

        /// <summary>
        /// Create a multiplexed client of the specified type to share the connection of this <see cref="KucoinBaseWebsocketFeed"/>-derived instance.
        /// </summary>
        /// <typeparam name="TFeed">The feed type to create.</typeparam>
        /// <typeparam name="TValue">The type of object that is pushed by the feed.</typeparam>
        /// <param name="inheritSymbols">True to initialize the child with all the active symbols of the current feed.</param>
        /// <returns>A new <see cref="SymbolTopicFeedBase{T}"/>.</returns>
        /// <remarks>
        /// TFeed must be an object derived from <see cref="SymbolTopicFeedBase{T}"/>.
        /// 
        /// An <see cref="InvalidOperationException"/> will be raised if an attempt is made to initialize a new feed on to a class instance that is already initialized as a multiplex client.
        /// </remarks>
        public virtual async Task<TFeed> CreateMultiplexedClient<TFeed, TValue>(bool inheritSymbols) where TFeed : SymbolTopicFeedBase<TValue>, new() where TValue: class, ISymbol, IStreamableObject
        {
            if (tunnelId != null && !isMultiplexHost)
            {
                throw new InvalidOperationException("Cannot initialize as multiplex connection host when already initialized as multiplex client.");
            }
            var child = new TFeed();

            if (!isMultiplexHost)
            {
                await MultiplexInit();
            }

            await child.MultiplexInit(this);

            if (inheritSymbols && Connected && activeSymbols?.Count > 0)
            {
                await child.SubscribeMany(activeSymbols);
            }

            return child;
        }


        #endregion

    }
}
