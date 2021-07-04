using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.OrderBooks
{


    /// <summary>
    /// Level 2 order book provider interface.
    /// </summary>
    /// <typeparam name="TBook">The type of order book (must implement <see cref="IOrderBook{T}"/> of <see cref="TUnit"/>.)</typeparam>
    /// <typeparam name="TUnit">The type of the order unit (must implement <see cref="IOrderUnit"/>.)</typeparam>
    /// <typeparam name="TUpdate">The type of the update object pushed with <see cref="IObserver{T}.OnNext(T)"/>.</typeparam>
    public interface ILevel3OrderBookProvider<TUpdate> :
        INotifyPropertyChanged,
        IDisposable,
        ISymbol,
        IObserver<TUpdate>
        where TUpdate : new()
    {

        /// <summary>
        /// The raw preflight (full depth) order book.
        /// </summary>
        KeyedAtomicOrderBook<AtomicOrderUnit> FullDepthOrderBook { get; }

        /// <summary>
        /// Reset and reinitialize the feed to trigger recalibration.
        /// </summary>
        void Reset();

        /// <summary>
        /// True if the object is disposed and no longer usable.
        /// </summary>
        bool Disposed { get; }

        bool Initialized { get; }

        bool Calibrated { get; }

        void Calibrate();

    }


    public abstract class Level3Base<TObservation> : SymbolTopicFeedBase<Level3Update>
        where TObservation: ILevel3OrderBookProvider<Level3Update>, new()
    {
        protected FeedState state = FeedState.Disconnected;

        protected object lockObj = new object();

        protected int resets;

        internal readonly Dictionary<string, TObservation> activeFeeds = new Dictionary<string, TObservation>();


        public Level3Base(ICredentialsProvider credProvider) : base(credProvider)
        {
            recvBufferSize = 131072;
        }

        public Level3Base(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            recvBufferSize = 131072;
        }

        public override bool IsPublic => true;

        protected override string Subject => throw new NotImplementedException();

        protected override string Topic => "/spotMarket/level3";

        /// <summary>
        /// Get the aggregated order book retrieval end point.
        /// </summary>
        public abstract string AggregateEndpoint { get; }

        /// <summary>
        /// Create a new instance of a class derived from <see cref="Level2ObservationBase{TBook, TUnit, TUpdate}"/>.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>A new observation.</returns>
        protected abstract TObservation CreateNewObservation(string symbol);

        protected abstract void SetOrderBook(TObservation observation, KeyedAtomicOrderBook<AtomicOrderUnit> value);

        protected abstract void SetInitialized(TObservation observation, bool value);

        protected abstract void SetCalibrated(TObservation observation, bool value);


        /// <summary>
        /// Gets the number of resets since the last subscription.
        /// </summary>
        public int Resets
        {
            get => resets;
            internal set
            {
                SetProperty(ref resets, value);
            }
        }


        public FeedState State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

        protected override void OnConnected()
        {
            _ = Ping();
            State = FeedState.Connected;
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            State = FeedState.Disconnected;
            base.OnDisconnected();
        }

        
        /// <summary>
        /// Get the full Level 2 Data Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 2 feed.
        /// </remarks>
        public virtual async Task<KeyedAtomicOrderBook<AtomicOrderUnit>> GetAggregatedOrder(string symbol)
        {
            var curl = AggregateEndpoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, 5, !IsPublic, param);
            var result = jobj.ToObject<KeyedAtomicOrderBook<AtomicOrderUnit>>();

            if (typeof(ISequencedOrderUnit).IsAssignableFrom(typeof(AtomicOrderUnit)))
            {
                foreach (var ask in result.Asks)
                {
                    ((ISequencedOrderUnit)ask).Sequence = result.Sequence;
                }

                foreach (var bid in result.Bids)
                {
                    ((ISequencedOrderUnit)bid).Sequence = result.Sequence;
                }
            }

            jobj = null;
            GC.Collect(2);

            return result;
        }
        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAggregatedOrder(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        protected virtual async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAggregatedOrder(af.Symbol);

            SetOrderBook(af, data);
            SetInitialized(af, true);

            Resets++;
        }

        long cycle = 0;

        protected override async Task HandleMessage(FeedMessage msg)
        {
            if (msg.Type == "message")
            {
                if (cycle == 0) cycle = DateTime.UtcNow.Ticks;

                var i = msg.Topic.IndexOf(":");

                if (i != -1)
                {
                    var symbol = msg.Topic.Substring(i + 1);

                    if (activeFeeds.TryGetValue(symbol, out TObservation af))
                    {
                        var update = msg.Data.ToObject<Level3Update>();
                        update.Subject = msg.Subject;

                        if (!af.Calibrated)
                        {
                            _ = Task.Run(() => State = FeedState.Initializing);

                            af.OnNext(update);

                            if ((DateTime.UtcNow.Ticks - cycle) >= (50 * 10_000))
                            {
                                await InitializeOrderBook(af.Symbol);

                                lock (lockObj)
                                {
                                    af.Calibrate();
                                    cycle = DateTime.UtcNow.Ticks;
                                }

                                _ = Task.Run(() =>
                                {
                                    State = FeedState.Running;
                                });

                            }
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                af.OnNext(update);

                                if ((DateTime.UtcNow.Ticks - cycle) >= (50 * 10_000))
                                {
                                    cycle = DateTime.UtcNow.Ticks;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
