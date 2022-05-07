using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Public
{
    public class Level2Direct : Level2
    {
        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level2Direct(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            wantPump = false;
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level2Direct(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox)
        {
            wantPump = false;
        }

        /// <summary>
        /// The data receive thread.
        /// </summary>
        protected override async void DataReceiveThread()
        {
            Thread.CurrentThread.Priority = recvPriority;

            byte[] inputChunk = new byte[chunkSize];

            StringBuilder sb = new StringBuilder();

            char inChar;

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
                        result = await valtask;
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
                    inChar = (char)inputChunk[i];
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
                            inEsc = false;
                        }
                        else if (inChar == '\"')
                        {
                            // quoted string complete, switch back to object scanning.
                            inQuote = false;
                        }
                    }
                    else if (inChar == '\"')
                    {
                        // quoted string avoidance logic
                        inQuote = true;
                    }
                    else if (inChar == '{')
                    {
                        // with quoted strings out of the way,
                        // all we have to do is count the JSON object nests.
                        ++level;
                    }
                    else if (inChar == '}')
                    {
                        --level;

                        if (level == 0)
                        {
                            // we're back down at the root level!
                            // we now have one whole JSON string to pass to the handler.

                            RouteJsonPacket(sb.ToString());

                            sb.Clear();
                            strlen = 0;
                        }
                    }
                }
            }
        }


    }
}
