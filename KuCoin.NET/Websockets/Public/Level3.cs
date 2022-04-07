using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets.Public;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Observations;
using System.Threading;
using KuCoin.NET.Json;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json.Linq;

namespace KuCoin.NET.Websockets.Public
{
    public interface ILevel3 : IMarketFeed<Level3OrderBook, Level3Update, KeyedAtomicOrderBook<AtomicOrderUnit>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>>
    {

    }

    public class Level3 : OrderBookFeed<Level3OrderBook, Level3Update, KeyedAtomicOrderBook<AtomicOrderUnit>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3>, ILevel3
    {

        public const int buy = -813464969;
        public const int sell = -1684090755;
        public const int filled = -1725622140;
        public const int canceled = -443854079;
        public const int sequence = 1384568619;
        public const int symbol = -322423047;
        public const int orderId = -98339785;
        public const int clientOid = 97372753;
        public const int side = 595663797;
        public const int price = -892853543;
        public const int size = -138402710;
        public const int remainSize = 513984757;
        public const int takerOrderId = -1263072696;
        public const int makerOrderId = 621206821;
        public const int tradeId = -154148381;
        public const int reason = 1001949196;
        public const int orderTime = -1047109151;
        public const int ts = -1014591861;
        public const int subject = -70369670;

        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level3(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

            recvBufferSize = 16 * 1024 * 1024;
            minQueueBuffer = 10000;
            chunkSize = 512;

            settings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[]
                {
                    new Level3UpdateConverter()
                }
            };
        }

        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level3(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox, futures: false)
        {
            recvBufferSize = 16 * 1024 * 1024;
            minQueueBuffer = 10000;
            chunkSize = 512;

            settings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[]
                {
                    new Level3UpdateConverter()
                }
            };
        }

        public override string Topic => "/spotMarket/level3";

        public override string InitialDataUrl => "/api/v3/market/orderbook/level3";

        public override bool IsPublic => false;

        public override void Release(IWebsocketListener obj) => Release((Level3OrderBook)obj);

        public void Release(Level3OrderBook obj)
        {
            if (activeFeeds.ContainsValue(obj))
            {
                try
                {
                    UnsubscribeOne(obj.Key).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {

                }
            }
        }

        protected override Level3OrderBook CreateFeed(string sym)
        {
            return new Level3OrderBook(this, sym);
        }
        
        public override async Task<KeyedAtomicOrderBook<AtomicOrderUnit>> ProvideInitialData(string key)
        {
            Exception err = null;

            var cts = new CancellationTokenSource();

            var ft = Task.Run(async () =>
            {
                var curl = InitialDataUrl;
                var param = new Dictionary<string, object>();

                param.Add("symbol", key);

                try
                {
                    var jobj = await MakeRequest(HttpMethod.Get, curl, auth: !IsPublic, reqParams: param);

                    var orderbook = new KeyedAtomicOrderBook<AtomicOrderUnit>();

                    var asks = jobj["asks"] as JArray;
                    var bids = jobj["bids"] as JArray;

                    orderbook.Asks.Capacity = asks.Count * 4;
                    orderbook.Bids.Capacity = bids.Count * 4;

                    jobj.Populate(orderbook);


                    GC.Collect(2);
                    return orderbook;
                }
                catch (Exception ex)
                {
                    err = ex;
                    return null;
                }

            }, cts.Token);

            DateTime start = DateTime.UtcNow;

            while ((DateTime.UtcNow - start).TotalSeconds < 60)
            {
                Thread.Sleep(10);

                if (ft.IsCompleted)
                {
                    return ft.Result;
                }
            }

            cts.Cancel();

            if (err != null)
            {
                Logger.Log(err.Message);
                throw err;
            }

            return null;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RouteJsonPacket(string json, FeedMessage e = null)
        {
            var msg = JsonConvert.DeserializeObject<Level3FeedMessage>(json, settings);

            if (msg.Type[0] == 'm' && msg.TunnelId == tunnelId)
            {
                var update = msg.Data;
                update.Subject = msg.Subject;

                activeFeeds[update.Symbol].OnNext(update);
            }
            else
            {
                base.RouteJsonPacket(json, e);
            }
        }

        public override int QueueLength => l3Queue.Count;
        List<Level3Update> l3Queue = new List<Level3Update>();

        protected override void MessagePumpThread()
        {
            string[] queue = new string[minQueueBuffer];
            Level3Update[] l3queue = new Level3Update[minQueueBuffer];
            Level3Update l3q;
            int c, d;

            Thread.CurrentThread.Priority = recvPriority;

            // loop forever
            while (!(ctsPump?.IsCancellationRequested ?? true) && socket?.State == WebSocketState.Open)
            {
                // lock on msgQueue.
                lock (msgQueue)
                {
                    c = msgQueue.Count;
                    d = l3Queue.Count;

                    if (c != 0)
                    {
                        if (queue.Length < c)
                        {
                            Array.Resize(ref queue, c * 2);
                        }

                        msgQueue.CopyTo(queue);
                        msgQueue.Clear();
                    }

                    if (d != 0)
                    {
                        if (l3queue.Length < d)
                        {
                            Array.Resize(ref l3queue, d * 2);
                        }

                        l3Queue.CopyTo(l3queue);
                        l3Queue.Clear();
                    }

                }

                if (c == 0 && d == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                for (int i = 0; i < c; i++)
                {
                    RouteJsonPacket(queue[i]);
                }

                for (int i = 0; i < d; i++)
                {
                    l3q = l3queue[i];
                    activeFeeds[l3q.Symbol].OnNext(l3q);
                }

                if (monitorThroughput)
                {
                    if (maxQueueLengthLast60Seconds < c)
                    {
                        maxQueueLengthLast60Seconds = c;
                    }
                }
            }

            msgQueue?.Clear();
            OnDisconnected();
        }

        /// <summary>
        /// The data receive thread.
        /// </summary>
        protected override async void DataReceiveThread()
        {
            Thread.CurrentThread.Priority = recvPriority;

            byte[] inputChunk = new byte[chunkSize];

            StringBuilder sb = new StringBuilder();

            Crc32 hash = new Crc32();

            char inChar;
            byte inByte;

            int strlen = 0;
            int level = 0;

            bool inQuote = false;
            bool inEsc = false;

            int i, c;
            int xlen = 0;

            sb.EnsureCapacity(recvBufferSize);
            DateTime xtime = DateTime.UtcNow;

            xtime.AddSeconds(-1 * xtime.Second);

            long tms = xtime.Ticks;
            long tqms = tms;
            int lastcrc = 0;
            StringBuilder cstr = new StringBuilder();
            cstr.Capacity = 50;
            bool maybenum = false;
            Level3Update update = new Level3Update();

#if DOTNETSTD
            WebSocketReceiveResult result;
            var memTarget = new ArraySegment<byte>(inputChunk);

            // loop forever or until the connection is broken or canceled.
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                try
                {
                    result = await socket.ReceiveAsync(memTarget, ctsReceive.Token);
                }
                catch
                {
                    return;
                }
#else
            ValueTask<ValueWebSocketReceiveResult> valtask;
            ValueWebSocketReceiveResult result;

            var memTarget = new Memory<byte>(inputChunk);

            // loop forever or until the connection is broken or canceled.
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                try
                {
                    valtask = socket.ReceiveAsync(memTarget, ctsReceive.Token);

                    if (valtask.IsCompletedSuccessfully)
                    {
                        result = valtask.Result;
                    }
                    else
                    {
                        result = await valtask.AsTask();
                    }
                }
                catch
                {
                    return;
                }
#endif
                if (ctsReceive?.IsCancellationRequested ?? true) return;

                c = result.Count;

                if (monitorThroughput)
                {
                    xtime = DateTime.UtcNow;

                    if ((DateTime.UtcNow.Ticks - tms) >= throughputUpdateInterval * 10_000)
                    {
                        Throughput = (long)(xlen * 8d * (1000d / throughputUpdateInterval));

                        tms = xtime.Ticks;

                        xlen = 0;
                    }
                    else
                    {
                        xlen += c;
                    }

                    if ((DateTime.UtcNow.Ticks - tqms) >= 600_000_000)
                    {
                        maxQueueLengthLast60Seconds = 0;

                        xtime.AddSeconds(-1 * xtime.Second);
                        tqms = xtime.Ticks;
                    }

                }

                strlen += c;

                sb.EnsureCapacity(strlen);

                // process as many complete JSON objects as we can and
                // hold on to incomplete string data for the next
                // receive, which will complete the current object
                // and add additional objects, ad infinitum.
                for (i = 0; i < c; i++)
                {
                    // character by character is the simplest and fastest way.
                    inByte = inputChunk[i];
                    inChar = (char)inByte;
                    sb.Append(inChar);

                    if (inQuote)
                    {
                        // quoted string logic

                        if (!inEsc && inChar == '\\')
                        {
                            // escaped character avoidance logic
                            inEsc = true;
                        }
                        else if (inEsc)
                        {
                            // escaped character avoided, continue scanning quoted string.
                            cstr.Append(inChar);
                            hash.Next(inByte);
                            inEsc = false;
                        }
                        else if (inChar == '\"')
                        {
                            var str = cstr.ToString();
                            if (str.Length > 0)
                            {
                                switch (lastcrc)
                                {
                                    case symbol:
                                        update.Symbol = str;
                                        break;

                                    case orderId:
                                        update.OrderId = str;
                                        break;

                                    case clientOid:
                                        update.ClientOid = str;
                                        break;

                                    case side:
                                        update.Side = (Side)hash.Current;
                                        break;

                                    case price:
                                        update.Price = decimal.Parse(str);
                                        break;

                                    case size:
                                        update.Size = decimal.Parse(str);
                                        break;

                                    case remainSize:
                                        update.RemainSize = decimal.Parse(str);
                                        break;

                                    case takerOrderId:
                                        update.TakerOrderId = str;
                                        break;

                                    case makerOrderId:
                                        update.MakerOrderId = str;
                                        break;

                                    case tradeId:
                                        update.TradeId = str;
                                        break;

                                    case reason:
                                        update.Reason = (DoneReason)hash.Current;
                                        break;

                                    case subject:
                                        update.Subject = str;
                                        break;
                                }
                            }

                            // quoted string complete, switch back to object scanning.
                            inQuote = false;
                            lastcrc = (int)hash.Reset();

                            cstr.Clear();
                        }
                        else
                        {
                            hash.Next(inByte);
                            cstr.Append(inChar);
                        }
                    }
                    else if (inChar == ':')
                    {
                        cstr.Clear();
                        hash.Reset();
                        
                        maybenum = true;
                    }
                    else if (inChar == '\"')
                    {
                        // quoted string avoidance logic
                        inQuote = true;
                        maybenum = false;

                        cstr.Clear();
                        hash.Reset();
                    }
                    else if (inChar == '{')
                    {
                        // with quoted strings out of the way,
                        // all we have to do is count the JSON object nests.
                        ++level;
                    }
                    else if (inChar == '}' || (maybenum && inChar != ' '))
                    {
                        if (maybenum)
                        {
                            if ("0123456789".Contains(inChar))
                            {
                                cstr.Append(inChar);
                            }
                            else
                            {
                                var ll = long.Parse(cstr.ToString());
                                switch (lastcrc)
                                {
                                    case sequence:
                                        update.Sequence = ll;
                                        break;

                                    case orderTime:
                                        update.OrderTime = EpochTime.NanosecondsToDate(ll);
                                        break;

                                    case ts:
                                        update.Timestamp = EpochTime.NanosecondsToDate(ll);
                                        break;
                                }

                                cstr.Clear();
                                maybenum = false;
                            }
                        }

                        if (inChar == '}')
                        {
                            --level;

                            if (level == 0)
                            {
                                // we're back down at the root level!
                                // we now have one whole JSON string to pass to the handler.

                                lock (msgQueue)
                                {
                                    if (update.Sequence > 0)
                                    {
                                        l3Queue.Add(update);
                                        update = new Level3Update();
                                    }
                                    else
                                    {
                                        msgQueue.Add(sb.ToString());
                                    }
                                    sb.Clear();
                                    strlen = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
