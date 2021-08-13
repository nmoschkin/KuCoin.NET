using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kucoin.NET.Websockets.Public
{
    public class Level3 : KucoinBaseWebsocketFeed
    {
        private Dictionary<string, Level3Observation> activeFeeds = new Dictionary<string, Level3Observation>();

        protected object lockObj = new object();

        protected int defaultPieces = 50;
        protected int updateInterval = 500;
        protected long updcalc = 500 * 10_000;
        
        protected long cycle = DateTime.UtcNow.Ticks;

        protected int resets;

        protected FeedState state = FeedState.Disconnected;

        public string Subject => throw new NotImplementedException();

        public string Topic => "/spotMarket/level3";

        public string OrderBookEndPoint => "/api/v3/market/orderbook/level3";


        /// <summary>
        /// Event that gets fired when the feed for a symbol has been calibrated and is ready to be used.
        /// </summary>
        public virtual event EventHandler SymbolCalibrated;

        public Level3(ICredentialsProvider credProvider) : base(credProvider)
        {
            recvBufferSize = 131072;
            minQueueBuffer = 500;
            chunkSize = 256;
        }

        public Level3(string key, string secret, string passphrase, bool isSandbox = false, bool futures = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: futures)
        {
            recvBufferSize = 131072;
            minQueueBuffer = 500;
            chunkSize = 256;
        }

        /// <summary>
        /// Gets or sets a length of time (in milliseconds) that indicates how often the order book is pushed to the UI thread.
        /// </summary>
        /// <remarks>
        /// The default value is 500 milliseconds.
        /// If this value is set to 0, automatic updates are disabled.
        /// </remarks>
        public virtual int UpdateInterval
        {
            get => updateInterval;
            set
            {
                // the minimum value is 5 milliseconds
                if (value < 5) value = 5;

                if (SetProperty(ref updateInterval, value))
                {
                    updcalc = updateInterval * 10_000;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default number of pieces for new observations.
        /// </summary>
        /// <remarks>
        /// The default value is 50 pieces.
        /// 
        /// To always include the full market depth, set this value to 0.
        /// 
        /// The value of this property only affects newly created observations.  Changing this value does not change the number of pieces
        /// in the live order books of observations that have already been created.  You can use the <see cref="ILevel2OrderBookProvider{TBook, TUnit, TUpdate}.Pieces"/> property
        /// on individual observations to change their default number of pieces.
        /// </remarks>
        public virtual int DefaultPieces
        {
            get => defaultPieces;
            set
            {
                SetProperty(ref defaultPieces, value);
            }
        }

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
            
        /// <summary>
        /// Gets the current feed state.
        /// </summary>
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

        public override bool IsPublic => false;


        /// <summary>
        /// Adds a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to subscribe.</param>
        /// <returns></returns>
        public async Task<Level3Observation> AddSymbol(string symbol)
        {
            var p = await AddSymbols(new string[] { symbol });
            return p[symbol];
        }

        /// <summary>
        /// Adds a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to subscribe.</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, Level3Observation>> AddSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, Level3Observation>();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!lnew.ContainsKey(sym))
                    {
                        lnew.Add(sym, activeFeeds[sym]);
                    }
                    continue;
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);

                var obs = new Level3Observation(this, sym);

                activeFeeds.Add(sym, obs);

                if (!lnew.ContainsKey(sym))
                {
                    lnew.Add(sym, activeFeeds[sym]);
                }
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "subscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

            State = FeedState.Subscribed;
            return lnew;
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbol(string symbol)
        {
            await RemoveSymbols(new string[] { symbol });
        }

        /// <summary>
        /// Remove a Level 2 subscription for the specified symbols.
        /// </summary>
        /// <param name="symbols">The symbols to remove.</param>
        /// <returns></returns>
        internal virtual async Task RemoveSymbols(IEnumerable<string> symbols)
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            var sb = new StringBuilder();

            foreach (var sym in symbols)
            {
                if (activeFeeds.ContainsKey(sym))
                {
                    if (!activeFeeds[sym].Disposed)
                    {
                        activeFeeds[sym].Dispose();
                    }

                    activeFeeds.Remove(sym);
                }

                if (sb.Length > 0) sb.Append(',');
                sb.Append(sym);
            }

            var topic = $"{Topic}:{sb}";

            var e = new FeedMessage()
            {
                Type = "unsubscribe",
                Id = connectId.ToString("d"),
                Topic = topic,
                Response = true,
                PrivateChannel = false
            };

            await Send(e);

            if (activeFeeds.Count == 0)
            {
                Resets = 0;
                State = FeedState.Unsubscribed;
            }
        }


        /// <summary>
        /// Get the full Level 3 Atomic Order Book for the specified trading symbol.
        /// </summary>
        /// <param name="symbol">The trading symbol.</param>
        /// <returns>The part book snapshot.</returns>
        /// <remarks>
        /// Returns the full market depth. 
        /// Use this to calibrate a full level 3 feed.
        /// </remarks>
        public virtual async Task<KeyedAtomicOrderBook<AtomicOrderStruct>> GetAtomicOrderBook(string symbol)
        {
            var curl = OrderBookEndPoint;
            var param = new Dictionary<string, object>();

            param.Add("symbol", symbol);

            var jobj = await MakeRequest(HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);
            var result = jobj.ToObject<KeyedAtomicOrderBook<AtomicOrderStruct>>();

            GC.Collect(2);
            return result;
        }

        /// <summary>
        /// Initialize the order book with a call to <see cref="GetAtomicOrderBook(string)"/>.
        /// </summary>
        /// <param name="symbol">The symbol to initialize.</param>
        /// <remarks>
        /// This method is typically called after the feed has been buffered.
        /// </remarks>
        internal virtual async Task InitializeOrderBook(string symbol)
        {
            if (!activeFeeds.ContainsKey(symbol)) return;

            var af = activeFeeds[symbol];

            var data = await GetAtomicOrderBook(af.Symbol);

            af.FullDepthOrderBook = data;
            af.Initialized = true;
            Resets++;       
        }

        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<FeedMessage<Level3Update>>(json);

            if (msg.TunnelId == tunnelId && msg.Type == "message")
            {
                var i = msg.Topic.IndexOf(":");

                if (i != -1)
                {
                    var symbol = msg.Topic.Substring(i + 1);
                    if (string.IsNullOrEmpty(symbol)) return;

                    var af = activeFeeds[symbol];
                    var update = msg.Data;
                    update.Subject = msg.Subject;

                    af.OnNext(update);

                }
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }

        }

        protected override Task HandleMessage(FeedMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
