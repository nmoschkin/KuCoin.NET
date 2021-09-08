using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Public;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kucoin.NET.Websockets.Distribution;
using Kucoin.NET.Websockets.Observations;
using System.Threading;
using Kucoin.NET.Json;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;

namespace Kucoin.NET.Websockets.Public
{
    
    public class Level3Direct : Level3
    {


        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level3Direct(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {

            recvBufferSize = 16777216;
            minQueueBuffer = 10000;
            chunkSize = 1024;
        }

        /// <summary>
        /// Instantiate a new Level 3 market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level3Direct(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox)
        {
            recvBufferSize = 16777216;
            minQueueBuffer = 10000;
            chunkSize = 1024;
        }

        protected override Level3OrderBook CreateFeed(string sym)
        {
            return new Level3OrderBook(this, sym, true);
        }

        public override async Task<IDictionary<string, Level3OrderBook>> SubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
            {
                await Connect();
            }

            var sb = new StringBuilder();
            var lnew = new Dictionary<string, Level3OrderBook>();

            lock (lockObj)
            {
                foreach (var sym in keys)
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

                    var obs = CreateFeed(sym);
                    activeFeeds.Add(sym, obs);

                    if (!lnew.ContainsKey(sym))
                    {
                        lnew.Add(sym, activeFeeds[sym]);
                    }
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

        public override async Task UnsubscribeMany(IEnumerable<string> keys)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            var sb = new StringBuilder();

            lock (lockObj)
            {
                foreach (var sym in keys)
                {
                    if (activeFeeds.ContainsKey(sym))
                    {
                        try
                        {
                            activeFeeds[sym].Dispose();
                        }
                        catch { }

                        activeFeeds.Remove(sym);
                    }

                    if (sb.Length > 0) sb.Append(',');
                    sb.Append(sym);
                }
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
                State = FeedState.Unsubscribed;
            }
        }


        /// <summary>
        /// The data receive thread.
        /// </summary>
        protected override void DataReceiveThread()
        {
            Thread.CurrentThread.Priority = recvPriority;

            byte[] inputChunk = new byte[chunkSize];

            StringBuilder sb = new StringBuilder();

            Crc32 hash = new Crc32();

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
            var arrSeg = new ArraySegment<byte>(inputChunk);

            // loop forever or until the connection is broken or canceled.
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                try
                {
                    result = socket.ReceiveAsync(arrSeg, ctsReceive.Token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
                catch
                {
                    return;
                }
#else
            ValueWebSocketReceiveResult result;
            var arrSeg = new Memory<byte>(inputChunk);

            // loop forever or until the connection is broken or canceled.
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                try
                {
                    result = socket.ReceiveAsync(arrSeg, ctsReceive.Token)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
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
                    byte inByte = inputChunk[i];
                    char inChar = (char)inByte;
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
                                        update.size = sb.Length;
                                        activeFeeds[update.Symbol].OnNext(update);
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
