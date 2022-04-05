/*****************************************************
 * KuCoin.NET Base Websocket Feed Class
 *
 * Basic structure and low-level communications for all
 * websocket feeds.
 *
 *
 * Copyright (C) 2021 Nathaniel Moschkin
 * Licensed under the Apache 2 license.
 *
 *
 *
 * **/

using KuCoin.NET.Data;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Rest;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Distribution.Contracts;
using KuCoin.NET.Websockets.Distribution.Services;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Websocket feed abstract base class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KucoinBaseWebsocketFeed :
        KucoinBaseRestApi,
        IAsyncPingable,
        IDisposable
    {

        #region Protected fields

        // this is only disabled for direct feeds.  
        // the code written for such a class needs to be swift and efficient.
        protected bool wantPump = true;

        protected ObservableCollection<Exception> errorLog = new ObservableCollection<Exception>();

        protected Exception lastError;

        protected KucoinBaseWebsocketFeed multiplexHost;

        protected bool isMultiplexHost = false;

        protected List<KucoinBaseWebsocketFeed> multiplexClients;

        protected string tunnelId = null;

        protected ClientWebSocket socket = null;

        protected TokenApplication token = null;

        protected CancellationTokenSource ctsReceive;

        protected CancellationTokenSource ctsSend;

        protected CancellationTokenSource ctsPump;

        protected Guid connectId = Guid.NewGuid();

        protected Server server;

        protected int throughputUpdateInterval = 1000;

        #region Connection Settings

        protected ThreadPriority recvPriority = ThreadPriority.Normal;

        protected bool recheckPriority;

        protected int chunkSize = 256;

        protected int recvBufferSize = 51920;

        protected int sendBufferSize = 51920;

        protected int minQueueBuffer = 512;

        protected bool monitorThroughput;

        protected long throughput;

        protected long maxQueueLengthLast60Seconds;

        protected TimeSpan pingTime = TimeSpan.Zero;

        protected DateTime lastPing = DateTime.Now;

        protected int? overridePingInterval = null;

        #endregion Connection Settings

        #endregion Protected fields

        #region Events

        /// <summary>
        /// Event that is raised when the client received a Pong response to a Ping.
        /// </summary>
        public virtual event EventHandler Pong;

        /// <summary>
        /// Event that is raised when the client has established a connection with the server.
        /// </summary>
        public virtual event EventHandler<FeedConnectedEventArgs> FeedConnected;

        /// <summary>
        /// Event that is raised when the client has disconnected from the server.
        /// </summary>
        /// <remarks>
        /// This event is raised when the object is manually disposed or disposed via the 'using' statement,
        /// but not when the destructor is called by the garbage collector.
        /// </remarks>
        public virtual event EventHandler<FeedDisconnectedEventArgs> FeedDisconnected;

        /// <summary>
        /// Event that is raised for every single JSON entity that is received.
        /// </summary>
        /// <remarks>
        /// Avoid using, if possible, as calling this handler will have a performance impact.
        /// </remarks>
        public virtual event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Event that is raised when an ack is received.
        /// </summary>
        public event EventHandler Ack;


        #endregion Events

        #region Static Constructor

        static KucoinBaseWebsocketFeed()
        {
            if (!Dispatcher.Initialized && SynchronizationContext.Current != null)
            {
                Dispatcher.Initialize(SynchronizationContext.Current);
            }
        }

        #endregion Static Constructor

        #region Default Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public KucoinBaseWebsocketFeed(
            ICredentialsProvider credProvider,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null,
            bool futures = false)
            : base(
                  credProvider,
                  url,
                  isv1api,
                  futures
                  )
        {
            if (multiplexHost != null)
            {
                Task<bool> t = MultiplexInit(multiplexHost);
                t.Wait();

                if (t.Result != true)
                {
                    throw new InvalidOperationException("Failed to initialize multiplexer from connection host.");
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public KucoinBaseWebsocketFeed(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null,
            bool futures = false)
            : base(
                  key,
                  secret,
                  passphrase,
                  isSandbox,
                  url,
                  isv1api,
                  futures
                  )
        {
            if (multiplexHost != null)
            {
                Task<bool> t = MultiplexInit(multiplexHost);
                t.Wait();

                if (t.Result != true)
                {
                    throw new InvalidOperationException("Failed to initialize multiplexer from connection host.");
                }
            }
        }

        #endregion Default Constructor

        #region Public Properties

        /// <summary>
        /// True if the websocket feed is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (multiplexHost != null)
                {
                    return multiplexHost.Connected;
                }
                else if (socket?.State == WebSocketState.Open)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets information about the server associated with this instance.
        /// </summary>
        public Server Server => server;

        /// <summary>
        /// Gets the token for this websocket connection.
        /// </summary>
        public TokenApplication Token => token;

        /// <summary>
        /// The connection Id for this connection.
        /// </summary>
        public Guid ConnectionId => connectId;

        /// <summary>
        /// In multiplexed connections, this is the Id of the current object's multiplex channel.
        /// </summary>
        public string TunnelId => tunnelId;

        /// <summary>
        /// For multiplexed connections, gets the object that owns the connection.
        /// </summary>
        public KucoinBaseWebsocketFeed MultiplexHost => multiplexHost;

        /// <summary>
        /// True if this object is the connection holder (host) of a multiplexed connection.
        /// </summary>
        public bool IsMultiplexHost => isMultiplexHost;

        /// <summary>
        /// Returns true if the current connection is multiplexed.
        /// </summary>
        public bool IsMultiplexed
        {
            get => tunnelId != null;
        }

        /// <summary>
        /// Gets the current throughput of the socket in bits per second.
        /// </summary>
        public virtual long Throughput
        {
            get => throughput;
            protected set
            {
                SetProperty(ref throughput, value);
            }
        }

        /// <summary>
        /// Gets or sets the interval, in milliseconds, for the throughput to update. Default is 1000 (1 second.)
        /// </summary>
        public virtual int ThroughputUpdateInterval
        {
            get => throughputUpdateInterval;
            set
            {
                SetProperty(ref throughputUpdateInterval, value);
            }
        }

        /// <summary>
        /// Gets the maximum queue length for the last 60 seconds.
        /// </summary>
        public virtual long MaxQueueLengthLast60Seconds => maxQueueLengthLast60Seconds;

        /// <summary>
        /// Enable throughput monitoring via the <see cref="Throughput"/> observable property.
        /// </summary>
        /// <remarks>
        /// Monitoring throughput can have a performance impact.  It is disabled, by default.
        /// </remarks>
        public virtual bool MonitorThroughput
        {
            get => monitorThroughput;
            set
            {
                SetProperty(ref monitorThroughput, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> of the last ping/pong.
        /// </summary>
        public TimeSpan PingTime
        {
            get => pingTime;
            private set
            {
                SetProperty(ref pingTime, value);
            }
        }

        /// <summary>
        /// If an exception occurs on calls to <see cref="HandleMessage(FeedMessage)"/>, they are logged here.
        /// </summary>
        /// <remarks>
        /// Exceptions are generally ignored on the message pump thread because the entire connection needs to be reset if an unhandled exception is raised.
        /// Exceptions are most likely to occur in <see cref="HandleMessage(FeedMessage)"/>.
        /// </remarks>
        public ObservableCollection<Exception> ErrorLog
        {
            get => errorLog;
        }

        /// <summary>
        /// The last exception thrown on <see cref="HandleMessage(FeedMessage)"/>.
        /// </summary>
        public Exception LastError
        {
            get => lastError;
            private set
            {
                SetProperty(ref lastError, value);
            }
        }

        /// <summary>
        /// Gets or sets the thread priority for the long-running data receive and message pump threads.
        /// </summary>
        /// <remarks>
        /// Setting the thread priority is not allowed while the socket is connected.
        /// The default value for this property is <see cref="ThreadPriority.Normal"/>.
        /// </remarks>
        public ThreadPriority ReceiveThreadPriority
        {
            get => recvPriority;
            set
            {
                if (Connected) throw new InvalidOperationException("Cannot set thread priority while connected.");
                SetProperty(ref recvPriority, value);
            }
        }

        #endregion Public Properties

        #region Public Abstract Members

        /// <summary>
        /// Gets the topic for the feed subscription.
        /// </summary>
        public abstract string Topic { get; }

        /// <summary>
        /// Gets a value indicating whether this feed is public, or not.
        /// </summary>
        public abstract bool IsPublic { get; }

        /// <summary>
        /// Handle a received message.
        /// </summary>
        /// <param name="msg">The <see cref="FeedMessage"/> object that was deserialized from JSON.</param>
        /// <returns></returns>
        protected abstract Task HandleMessage(FeedMessage msg);

        #endregion Public Abstract Members

        #region Connection Handling

        /// <summary>
        /// Disconnect the websocket.
        /// </summary>
        /// <remarks>
        /// This method does not dispose any <see cref="IObserver{T}"/> subscriptions.
        /// </remarks>
        public virtual void Disconnect()
        {
            socket?.Dispose();
            socket = null;
            token = null;

            CancelAllThreads();

            OnPropertyChanged(nameof(Connected));
        }

        /// <summary>
        /// Apply for a websocket token and endpoint.
        /// </summary>
        /// <returns>The token application response.</returns>
        protected async Task<TokenApplication> ApplyForToken()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);

            string endPoint = "/api/v1/bullet-" + (IsPublic ? "public" : "private");

            var result = await MakeRequest(HttpMethod.Post, endPoint, wholeResponseJson: true, auth: !IsPublic);

            var subData = result.ToObject<TokenApplication>();

            token = subData;
            server = token.Data.Servers[0];

            return subData;
        }


        /// <summary>
        /// Establish a websocket connection to the remote server.
        /// </summary>
        /// <returns>True if the connection was negotiated and established, successfully.</returns>
        public virtual async Task<bool> Connect()
        {
            return await Connect(false);
        }

        /// <summary>
        /// Establish a websocket connection to the remote server.
        /// </summary>
        /// <param name="initAsMultiplexHost">True to initialize as a multiplexing tunnel connection host.</param>
        /// <param name="tunnelId">Optional tunnel id for initialization.  If one is not specified, one will be created.</param>
        /// <returns>True if the connection was negotiated and established, successfully.</returns>
        public async Task<bool> Connect(bool initAsMultiplexHost, string tunnelId = null)
        {
            if (socket != null)
            {
                try
                {
                    socket.Dispose();
                }
                catch
                {
                    socket = null;
                }

            }

            socket = new ClientWebSocket();

            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (Connected) return false;

            ctsReceive = new CancellationTokenSource();
            ctsSend = new CancellationTokenSource();
            ctsPump = new CancellationTokenSource();

            if (token == null)
            {
                try
                {
                    await ApplyForToken();
                }
                catch
                {
                    return false;
                }
            }

            var uri = $"{server.EndPoint}?token={token.Data.Token}&connectId={connectId:d}";

            socket.Options.SetBuffer(recvBufferSize, sendBufferSize);

            await socket.ConnectAsync(new Uri(uri), ctsReceive.Token);

            if (socket?.State == WebSocketState.Open)
            {
                // The Pinger:
                PingService.RegisterService(this);

                // The Reader:
                // -----------

                if (wantPump)
                {
                    // message queue
                    msgQueue = new List<string>();
                    msgQueue.Capacity = minQueueBuffer;

                    EnableMessagePumpThread();
                }

                // data receiver
                inputReaderThread = new Thread(DataReceiveThread);
                inputReaderThread.IsBackground = true;

                inputReaderThread.Start();

                if (initAsMultiplexHost)
                {
                    await MultiplexInit(tunnelId);
                }

                OnPropertyChanged(nameof(Connected));
                OnConnected();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Called when the websocket connection is established.
        /// </summary>
        /// <remarks>
        /// The default behavior of this method is to invoke the <see cref="FeedConnected"/> event.
        /// </remarks>
        protected virtual void OnConnected()
        {
            FeedConnected?.Invoke(this, new FeedConnectedEventArgs(connectId, server, token.Data.Token));
        }

        /// <summary>
        /// Called when the websocket connection is terminated.
        /// </summary>
        /// <remarks>
        /// The default behavior of this method is to invoke the <see cref="FeedDisconnected"/> event.
        /// </remarks>
        protected virtual void OnDisconnected()
        {
            FeedDisconnected?.Invoke(this, new FeedDisconnectedEventArgs(socket?.CloseStatus ?? WebSocketCloseStatus.ProtocolError));
        }

        #endregion Connection Handling

        #region Multiplexing

        // The general rules for multiplexing are that you cannot
        // multiplex futures and non-futures based objects together,
        // and you cannot initialize a private client tunnel with
        // a public tunnel host.

        /// <summary>
        /// Open a new tunnel in the feed.
        /// </summary>
        /// <param name="tunnelId">The new unique tunnel ID (usually a GUID).</param>
        protected async Task OpenMultiplexChannel(string tunnelId)
        {
            var m = new FeedMessage()
            {
                Id = connectId.ToString("d"),
                Type = "openTunnel",
                NewTunnelId = tunnelId,
                Response = true
            };

            await Send(m.ToString());
        }

        /// <summary>
        /// Close a multiplex tunnel.
        /// </summary>
        /// <param name="tunnelId">The unique tunnel ID of the tunnel to close.</param>
        protected async Task CloseMultiplexChannel(string tunnelId)
        {
            var m = new FeedMessage()
            {
                Id = connectId.ToString("d"),
                Type = "closeTunnel",
                TunnelId = tunnelId,
                Response = true
            };

            await Send(m.ToString());
        }

        /// <summary>
        /// Cancel all tunnel subscriptions and remove all
        /// multiplexed tunnels (including host and clients).
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> RemoveMultiplexChannels()
        {
            if (isMultiplexHost)
            {
                foreach (var client in multiplexClients)
                {
                    await client.RemoveMultiplexChannels();
                }
            }

            if (tunnelId != null)
            {
                await CloseMultiplexChannel(tunnelId);
            }

            tunnelId = null;
            return true;
        }

        /// <summary>
        /// Initialize multiplexing as the host.
        /// </summary>
        /// <returns>True if successful</returns>
        /// <remarks>
        /// This method must be called after establishing a connection using the <see cref="Connect(bool, string)"/> method.
        ///
        /// This method may fail if the websocket is not connected, the class instance is already a multiplex host, or
        /// the class instance is already attached as a multiplex client.
        /// </remarks>
        public virtual async Task<bool> MultiplexInit(string newTunnelId = null)
        {
            if (!Connected || isMultiplexHost || multiplexHost != null) return false;

            isMultiplexHost = true;
            tunnelId = newTunnelId ?? Guid.NewGuid().ToString("d");
            multiplexClients = new List<KucoinBaseWebsocketFeed>();

            await OpenMultiplexChannel(tunnelId);

            return true;
        }

        /// <summary>
        /// Create a multiplexed client of the specified type to share the connection of this <see cref="KucoinBaseWebsocketFeed"/>-derived instance.
        /// </summary>
        /// <typeparam name="TFeed">The feed type to create</typeparam>
        /// <returns>A new feed that shares a connection with the current object, or null if an error occurred.</returns>
        /// <remarks>
        /// A connection must already be established to use this method.
        /// An <see cref="InvalidOperationException"/> will be raised if the class instance is already initialized as a multiplex client.
        /// An <see cref="InvalidOperationException"/> will be raised if an attempt is made to initialized a private multiplex client from a public multiplex connection host.
        /// </remarks>
        public virtual async Task<TFeed> CreateMultiplexClient<TFeed>() where TFeed : KucoinBaseWebsocketFeed, new()
        {
            if (tunnelId != null && !isMultiplexHost)
            {
                throw new InvalidOperationException("Cannot initialize as multiplex connection host when already initialized as multiplex client.");
            }

            var client = new TFeed();

            if (IsFutures != client.IsFutures)
            {
                throw new InvalidOperationException("Cannot mix Futures and non-Futures API objects.");
            }

            if (IsPublic && !client.IsPublic)
            {
                throw new InvalidOperationException("Cannot initialize a private multiplex client from a public multiplex connection host.");
            }
            if (!Connected)
            {
                return null;
            }
            else if (!isMultiplexHost)
            {
                await MultiplexInit();
            }

            await client.MultiplexInit(this);
            return client;
        }

        /// <summary>
        /// Initialize multiplexing as a client.
        /// </summary>
        /// <param name="host"></param>
        /// <remarks>
        /// An <see cref="InvalidOperationException"/> will be raised if an attempt is made to initialized a private multiplex client from a public multiplex connection host.
        /// </remarks>
        public virtual async Task<bool> MultiplexInit(KucoinBaseWebsocketFeed host, string newTunnelId = null)
        {
            if (IsFutures != host.IsFutures)
            {
                throw new InvalidOperationException("Cannot mix Futures and non-Futures API objects.");
            }

            if (!IsPublic && host.IsPublic)
            {
                throw new InvalidOperationException("Cannot initialize a private multiplex client from a public multiplex connection host.");
            }

            if (Connected || tunnelId != null)
            {
                return false;
            }

            tunnelId = newTunnelId ?? Guid.NewGuid().ToString("d");
            multiplexHost = host;

            if (!host.IsMultiplexHost)
            {
                if (!await host.MultiplexInit()) return false;
            }
            else if (host.multiplexClients.Contains(this))
            {
                return false;
            }
            else if (host.multiplexClients.Count >= 4)
            {
                return false;
            }

            host.multiplexClients.Add(this);
            await host.OpenMultiplexChannel(tunnelId);

            return true;
        }

        #endregion Multiplexing

        #region Data Send and Receive

        // The core engine is private and cannot (and should not) be altered by derived classes.

        protected List<string> msgQueue;

        protected Thread inputReaderThread;

        protected Thread msgPumpThread;
        
        public virtual int QueueLength => msgQueue?.Count ?? 0;

        /// <summary>
        /// The data receive thread.
        /// </summary>
        protected virtual async void DataReceiveThread()
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

                            lock(msgQueue)
                            {
                                msgQueue.Add(sb.ToString());

                                sb.Clear();
                                strlen = 0;
                            }
                        }
                    }
                }
            }
        }

        protected void EnableMessagePumpThread()
        {
            // observer notification pump
            ctsPump = new CancellationTokenSource();
            msgPumpThread = new Thread(MessagePumpThread);
            msgPumpThread.IsBackground = true;
            msgPumpThread.Start();
        }

        protected void DisableMessagePumpThread()
        {
            ctsPump?.Cancel();
            msgPumpThread = null;
            ctsPump = null;
        }

        /// <summary>
        /// Separate thread that runs to pump messages to the observers in
        /// the order in which they were received without delaying the data
        /// receiving thread.
        /// </summary>
        protected virtual void MessagePumpThread()
        {
            string[] queue = new string[minQueueBuffer];
            int c;

            Thread.CurrentThread.Priority = recvPriority;

            // loop forever
            while (!(ctsPump?.IsCancellationRequested ?? true) && socket?.State == WebSocketState.Open)
            {
                // lock on msgQueue.
                lock (msgQueue)
                {
                    c = msgQueue.Count;

                    if (c != 0)
                    {
                        if (queue.Length < c)
                        {
                            Array.Resize(ref queue, c * 2);
                        }

                        msgQueue.CopyTo(queue);
                        msgQueue.Clear();
                    }

                }

                if (c == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                for (int i = 0; i < c; i++)
                {
                    RouteJsonPacket(queue[i]);
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
        /// Handles initial processing and routing of JSON packets.
        /// </summary>
        /// <param name="json">The JSON string that was received from the remote endpoint.</param>
        /// <param name="e">(Optional) Feed message decoded from multiplex host.</param>
        /// <remarks>
        /// This method deserializes the JSON data and calls either the
        /// <see cref="OnPong(FeedMessage)"/> or <see cref="HandleMessage(FeedMessage)"/> methods, and the <see cref="OnJsonReceived(string)"/> method.
        /// OnJsonReceive is called after all other handling has occurred.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RouteJsonPacket(string json, FeedMessage e = null)
        {
            if (e == null) e = JsonConvert.DeserializeObject<FeedMessage>(json);

            if (e.Type == "pong")
            {
                _ = Task.Run(() => OnPong(e), ctsReceive?.Token ?? default);
                OnJsonReceived(json);
            }
            else
            {
                if (tunnelId == e.TunnelId)
                {
                    try
                    {
                        HandleMessage(e).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch(Exception ex)
                    {
                        lock(errorLog)
                        {
                            errorLog.Add(ex);
                            LastError = ex;
                        }
                    }

                    // the tunnel destination will fire this.
                    OnJsonReceived(json);
                }
                else if (isMultiplexHost)
                {
                    var p = multiplexClients.Where((client) => client.tunnelId == e.TunnelId).FirstOrDefault();

                    if (p != null)
                    {
                        // pass the deserialized object to the client 
                        // so it doesn't need to be deserialized again.
                        p.RouteJsonPacket(json, e);
                    }
                }
            }
        }

        /// <summary>
        /// Called whenever a JSON data packet is received.
        /// </summary>
        /// <param name="json">The raw JSON string.</param>
        /// <remarks>
        /// The default behavior of this method is to fire the <see cref="DataReceived"/> event.
        /// This method is only invoked on the destination tunnel.  The multiplex host will pass through.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnJsonReceived(string json)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(json));
        }

        /// <summary>
        /// Called when a pong (a response to a ping) is received.
        /// </summary>
        /// <param name="msg">The contents of the pong message.</param>
        /// <remarks>
        /// You may override this function to do periodic work in your own application.
        ///
        /// The default behavior of this method is to record the ping time, and fire the <see cref="Pong"/> event.
        /// </remarks>
        protected virtual void OnPong(FeedMessage msg)
        {
            PingTime = DateTime.Now - lastPing;
            Pong?.Invoke(this, new EventArgs());
        }

        protected object ackLock = new object();
        protected bool awaitAck = false;

        /// <summary>
        /// Called when an ack message is received.  
        /// </summary>
        /// <param name="msg">The contents of the ack message.</param>
        /// <remarks>
        /// You may override this function to confirm requests.
        ///
        /// The default behavior of this method is to fire the <see cref="Ack"/> event.
        /// </remarks>
        protected virtual void OnAck(FeedMessage msg)
        {
            lock (ackLock)
            {
                if (awaitAck)
                {
                    awaitAck = false;
                }
            }
            
            Ack?.Invoke(this, new EventArgs());
        }
        
        /// <summary>
        /// Waits for an ack.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <returns></returns>
        public async Task<bool> AwaitAck(int timeout = 1000)
        {
            return await Task.Run(() =>
            {
                var t = DateTime.Now;

                lock (ackLock)
                {
                    awaitAck = true;
                }

                while ((DateTime.Now - t).TotalMilliseconds <= timeout)
                {
                    if (!awaitAck) return true;
                    Thread.Sleep(1);
                }

                return false;
            });
        }

        /// <summary>
        /// Send binary data to the remote server.
        /// </summary>
        /// <param name="data">Raw data bytes to send.</param>
        protected async Task Send(byte[] data)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            
            if (multiplexHost != null)
            {
                await multiplexHost.socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, multiplexHost.ctsSend.Token);
            }
            else
            {
                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, ctsSend.Token);
            }
        }

        /// <summary>
        /// Send text to the remote server.
        /// </summary>
        /// <param name="text">The text string to send.</param>
        /// <param name="encoding">The optional <see cref="Encoding"/> to use to transmit the text.
        /// If this parameter is null, UTF-8 encoding is used, by default.
        /// </param>
        protected async Task Send(string text, Encoding encoding = null)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected) return;

            if (encoding == null) encoding = Encoding.UTF8;

            var data = encoding.GetBytes(text);

            await Send(data);
        }

        /// <summary>
        /// Send a feed message object to the remote server.
        /// </summary>
        /// <param name="data">The data object to send.</param>
        /// <param name="encoding">The optional <see cref="Encoding"/> to use to transmit the text.
        /// If this parameter is null, UTF-8 encoding is used, by default.
        /// </param>
        /// <remarks>The data object is serialized to an encoded JSON string to be sent to the server.</remarks>
        protected async Task Send(FeedMessage data, Encoding encoding = null)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);

            if (tunnelId != null && data.Type != "openTunnel")
            {
                data.TunnelId = tunnelId;
            }

            if (!IsPublic && data.PrivateChannel == null)
            {
                data.PrivateChannel = true;
            }

            await Send(data.ToString(), encoding);
        }

#region IAsyncPingable

        /// <summary>
        /// Ping the remote connection.
        /// </summary>
        /// <returns>A reply will come from the server as a 'pong' message.</returns>
        public async Task Ping()
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().FullName);
            var e = new FeedMessage();

            e.Id = connectId.ToString("d");
            e.Type = "ping";

            lastPing = DateTime.Now;
            await Send(e);
        }

        void IPingable.Ping()
        {
            Ping().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public int PingInterval
        {
            get => overridePingInterval ?? server?.PingInterval ?? 0;
            set
            {
                // so, if the server hasn't been set, yet, we don't have a ping interval to override, yet.
                // and, when the server is finally set, it will reset the override, anyway,
                // and that is the rule we are going to enforce, here,
                // so we only set the ping interval if a server exists.
                if (server != null)
                {
                    // It is not safe to have a ping interval greater than the server-recommended interval, as the server could drop the connection.
                    if (value > server.PingInterval) throw new ArgumentOutOfRangeException("It is not safe to have a ping interval greater than the server-recommended interval, as the server could drop the connection.");

                    // if the override is the same as the server interval, we do not set it.
                    if (value != (overridePingInterval ?? server.PingInterval))
                    {
                        // if the override is the same as the server interval, we set it null.
                        if (value == server.PingInterval) overridePingInterval = null;
                        else overridePingInterval = value;

                        OnPropertyChanged(nameof(PingInterval));
                    }
                }
            }
        }

#endregion IAsyncPingable

#endregion Data Send and Receive

#region Thread cancellation

        /// <summary>
        /// Cancel all threads.
        /// </summary>
        protected void CancelAllThreads()
        {
            CancelReceiveThread();
            CancelPumpThread();
            CancelSendThread();
        }

        /// <summary>
        /// Cancel the receive thread.
        /// </summary>
        protected void CancelReceiveThread()
        {
            ctsReceive?.Cancel();
        }

        /// <summary>
        /// Cancel any active send thread.
        /// </summary>
        protected void CancelSendThread()
        {
            ctsSend?.Cancel();
        }

        /// <summary>
        /// Cancel the ping thread.
        /// </summary>
        protected void CancelPumpThread()
        {
            ctsPump?.Cancel();
        }

#endregion Thread cancellation

#region IDisposable Pattern

        protected bool disposedValue = false;

        protected bool disposing = false;

        /// <summary>
        /// True if this object has been disposed.
        /// </summary>
        public bool Disposed => disposedValue;

        ~KucoinBaseWebsocketFeed()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of this object, canceling all subscriptions and closing the websocket connection.
        /// </summary>
        /// <remarks>
        /// After disposal, the object becomes unusable and cannot connect, again.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">True if being called from the <see cref="Dispose"/> method, false if being called by the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (this.disposing) return;
            this.disposing = true;

            PingService.UnregisterService(this);

            Disconnect();
            CancelAllThreads();
            
            if (isMultiplexHost && multiplexClients != null && multiplexClients.Count > 0)
            {
                // multiplexed clients need to go, too.
                foreach (var client in multiplexClients)
                {
                    client.Dispose();
                }
            }

            socket?.Dispose();

            socket = null;
            token = null;
            tunnelId = null;

            disposedValue = true;

            if (disposing)
            {
                OnPropertyChanged(nameof(Connected));
            }

            this.disposing = false;
        }

#endregion IDisposable Pattern
    }

    /// <summary>
    /// Websocket feed abstract base class for <see cref="IObservable{T}"/> implementations.
    /// </summary>
    /// <typeparam name="T">The type of object to serve.</typeparam>
    public abstract class KucoinBaseWebsocketFeed<T> :
        KucoinBaseWebsocketFeed,
        IWebsocketFeed<T>,
        IObservable<T>
        where T : class, IStreamableObject
    {
       
        /// <summary>
        /// Event that is raised for every single JSON entity that is received.
        /// </summary>
        /// <remarks>
        /// Avoid using, if possible, as calling this handler will have a performance impact.
        /// </remarks>
        public virtual event EventHandler<FeedDataReceivedEventArgs<T>> FeedDataReceived;

        /// <summary>
        /// Gets the message subject for the feed cycle.
        /// </summary>
        public abstract string Subject { get; }

#region Default Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is Sandbox Mode</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public KucoinBaseWebsocketFeed(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null,
            bool futures = false) : base(key, secret, passphrase, isSandbox, url, isv1api, multiplexHost, futures)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
        /// <param name="futures">Use the Futures API endpoint.</param>
        public KucoinBaseWebsocketFeed(
            ICredentialsProvider credProvider,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null,
            bool futures = false) : base(credProvider, url, isv1api, multiplexHost, futures)
        {
        }

#endregion Default Constructor

#region IObservable<T> Pattern

        internal List<FeedObject<T>> observations = new List<FeedObject<T>>();

        /// <summary>
        /// Subscribe to this feed.
        /// </summary>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observations)
            {
                foreach (var obs in observations)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new FeedObject<T>(this, observer);
                observations.Add(obsNew);

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
        internal virtual void RemoveObservation(FeedObject<T> observation)
        {
            Release(observation);
        }

        /// <summary>
        /// Push the deserialized subscription object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual async Task PushNext(T obj)
        {
            if (disposedValue) throw new ObjectDisposedException(GetType().Name);

            // TODO - This needs rewriting.
            await Task.Run(() =>
            {
                List<Action> parallelActions = new List<Action>();

                foreach (var obs in observations)
                {
                    parallelActions.Add(() => obs.Observer.OnNext(obj));
                }

                if (FeedDataReceived != null)
                {
                    parallelActions.Add(() => FeedDataReceived.Invoke(this, new FeedDataReceivedEventArgs<T>(obj)));
                }

                Parallel.Invoke(parallelActions.ToArray());

            });
        }

#endregion IObservable<T> Pattern

        protected override void Dispose(bool disposing)
        {
            socket?.Dispose();

            try
            {
                if (Monitor.TryEnter(observations))
                {
                    try
                    {
                        var c = observations.Count;

                        for (int i = 0; i < c; i++)
                        {
                            try
                            {
                                if (i >= observations.Count) break;
                                observations[i]?.Dispose();
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        Monitor.Exit(observations);
                    }

                }
            }
            catch { }

            base.Dispose(disposing);
        }

        public IEnumerable GetActiveFeeds() => observations;

        public void Release(IWebsocketListener obj)
        {
            if (obj is FeedObject<T> observation)
            {
                observation.Observer.OnCompleted();

                lock (observations)
                {
                    if (observations.Contains(observation))
                    {
                        observations.Remove(observation);
                    }
                }
            }
        }

        IEnumerable<FeedObject<T>> IWebsocketFeed<T, FeedObject<T>>.GetActiveFeeds()
        {
            return observations;
        }
    }
}