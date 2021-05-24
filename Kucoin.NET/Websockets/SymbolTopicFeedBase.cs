﻿using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Data.Market;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Base class for live feeds supporting multiple symbols per feed.
    /// </summary>
    public abstract class SymbolTopicFeedBase<T> : KucoinBaseWebsocketFeed<T> where T: class, ISymbol
    {
        protected List<string> activeSymbols = new List<string>();

        protected string subject;
        
        protected string topic;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        public SymbolTopicFeedBase(
          string key,
          string secret,
          string passphrase,
          bool isSandbox = false)
          : base(key, secret, passphrase, isSandbox)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        public SymbolTopicFeedBase(ICredentialsProvider credProvider) : base(credProvider)
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
                        symbol.SetSymbol(msg.Topic.Substring(i + 1));
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
        public async Task AddSymbol(string symbol)
        {
            await AddSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Add to the specified symbols
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public virtual async Task AddSymbols(IEnumerable<string> symbols)
        {
            if (subject == null) subject = Subject;
            if (this.topic == null) this.topic = Topic;

            if (disposed) throw new ObjectDisposedException(nameof(SymbolTopicFeedBase<T>));
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

            var topic = $"{this.topic}:{sb}";


            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true
            };

            await Send(e);

        }

        /// <summary>
        /// Remove from the specified symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public virtual async Task RemoveSymbol(string symbol)
        {
            await RemoveSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Remove from the specified symbols
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public virtual async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(nameof(SymbolTopicFeedBase<T>));
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (!activeSymbols.Contains(sym)) continue;

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                activeSymbols.Remove(sym);
            }

            var topic = $"{this.topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true
            };

            await Send(e);
        }

        /// <summary>
        /// Remove from all symbols
        /// </summary>
        /// <returns></returns>
        public virtual async Task RemoveAllSymbols()
        {
            await RemoveSymbols(activeSymbols);
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
        public IDisposable Subscribe(IObserver<T> observer, IEnumerable<string> symbols)
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
            if (observations == null || observations.Count == 0) return;

            await Task.Run(() =>
            {
                foreach (SymbolObservation<T> obs in observations)
                {
                    if (obs.ActiveSymbols.Count == 0 || obs.ActiveSymbols.Contains(obj.Symbol))
                    {
                        obs.Observer.OnNext(obj);
                    }
                }
            });
        }

        #endregion


        #region Multiplexing

        /// <summary>
        /// Create a multiplexed client of the specified type to share the connection of this <see cref="KucoinBaseWebsocketFeed"/>-derived instance.
        /// </summary>
        /// <typeparam name="TFeed">The feed type to create.</typeparam>
        /// <typeparam name="TObj">The type of object that is pushed by the feed.</typeparam>
        /// <param name="inheritSymbols">True to initialize the child with all the active symbols of the current feed.</param>
        /// <returns>A new <see cref="SymbolTopicFeedBase{T}"/>.</returns>
        /// <remarks>
        /// TFeed must be an object derived from <see cref="SymbolTopicFeedBase{T}"/>.
        /// 
        /// An <see cref="InvalidOperationException"/> will be raised if an attempt is made to initialize a new feed on to a class instance that is already initialized as a multiplex client.
        /// </remarks>
        public virtual async Task<TFeed> CreateMultiplexedClient<TFeed, TObj>(bool inheritSymbols) where TFeed : SymbolTopicFeedBase<TObj>, new() where TObj: class, ISymbol
        {
            if (tunnelId != null && !isMultiplexHost)
            {
                throw new InvalidOperationException("Cannot initialize as multiplex client when already initialized as multiplex connection host.");
            }
            var child = new TFeed();

            if (!isMultiplexHost)
            {
                await MultiplexInit();
            }

            await child.MultiplexInit(this);

            if (inheritSymbols && Connected && activeSymbols?.Count > 0)
            {
                await child.AddSymbols(activeSymbols);
            }

            return child;
        }


        #endregion

    }
}