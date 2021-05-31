
/*****************************************************
 * Kucoin.NET Base Websocket Feed Class 
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


using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Kucoin.NET.Data.Websockets;

using System.Net.Http;
using System.Net.WebSockets;
using Kucoin.NET.Rest;
using Kucoin.NET.Helpers;

namespace Kucoin.NET.Websockets
{

    /// <summary>
    /// Websocket feed abstract base class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KucoinBaseWebsocketFeed :
        KucoinBaseRestApi,
        IDisposable
    {
        #region Protected fields

        protected KucoinBaseWebsocketFeed multiplexHost;

        protected bool isMultiplexHost = false;

        protected List<KucoinBaseWebsocketFeed> multiplexClients;

        protected string tunnelId = null;

        protected ClientWebSocket socket = null;

        protected TokenApplication token = null;

        protected CancellationTokenSource ctsReceive;

        protected CancellationTokenSource ctsSend;

        protected CancellationTokenSource ctsPing;

        protected Guid connectId = Guid.NewGuid();

        protected Server server;

        protected int chunkSize = 256;

        protected int recvBufferSize = 51920;

        protected int sendBufferSize = 51920;

        #endregion

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
        public virtual event EventHandler FeedDisconnected;

        /// <summary>
        /// Event that is raised for every single JSON entity that is received.
        /// </summary>
        /// <remarks>
        /// Avoid using, if possible, as calling this handler will have a performance impact.
        /// </remarks>
        public virtual event EventHandler<DataReceivedEventArgs> DataReceived;

        #endregion

        #region Static Constructor

        static KucoinBaseWebsocketFeed()
        {
            if (!Dispatcher.Initialized && SynchronizationContext.Current != null)
            {
                Dispatcher.Initialize(SynchronizationContext.Current);
            }
        }

        #endregion

        #region Default Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
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
                    throw new InvalidOperationException("Failed to initialize multiplexer from parent.");
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
                    throw new InvalidOperationException("Failed to initialize multiplexer from parent.");
                }
            }
        }

        #endregion

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

        #endregion

        #region Public Abstract Members

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

        #endregion

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

            OnPropertyChanged(nameof(Connected));
            FeedDisconnected?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Apply for a websocket token and endpoint.
        /// </summary>
        /// <returns>The token application response.</returns>
        protected async Task<TokenApplication> ApplyForToken()
        {
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));

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
        /// <param name="initAsMultiplexHost">True to initialize as a multiplexing tunnel connection host.</param>
        /// <param name="tunnelId">Optional tunnel id for initialization.  If one is not specified, one will be created.</param>
        /// <returns>True if the connection was negotiated and established, successfully.</returns>
        public async Task<bool> Connect(bool initAsMultiplexHost = false, string tunnelId = null)
        {
            if (socket == null) socket = new ClientWebSocket();

            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));
            if (Connected) return false;

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

            ctsReceive = new CancellationTokenSource();
            ctsSend = new CancellationTokenSource();
            ctsPing = new CancellationTokenSource();

            var uri = $"{server.EndPoint}?token={token.Data.Token}&connectId={connectId:d}";

            socket.Options.SetBuffer(recvBufferSize, sendBufferSize);

            await socket.ConnectAsync(new Uri(uri), ctsReceive.Token);

            if (socket.State == WebSocketState.Open)
            {
                // The Pinger: 

                keepAlive = Task.Factory.StartNew(
                    async () =>
                    {
                        int pi = server.PingInterval;

                        while (!ctsPing.IsCancellationRequested && socket?.State == WebSocketState.Open)
                        {
                            for (int i = 0; i < pi; i += 1000)
                            {
                                await Task.Delay(1000);
                                if (ctsPing.IsCancellationRequested) return;
                            }

                            await Ping();
                        }

                    }, 
                    TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
                );

                // The Reader: 
                // -----------

                // message queue
                msgQueue = new List<string>();

                // data receiver
                inputReaderThread = Task.Factory.StartNew(
                    DataReceiveThread, 
                    TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
                    );

                // observer notification pump
                msgPumpThread = Task.Factory.StartNew(
                    MessagePumpThread, 
                    TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
                    );

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

        #endregion

        #region Multiplexing

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
                foreach (var child in multiplexClients)
                {                    
                    await child.RemoveMultiplexChannels();
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
        public virtual async Task<TFeed> CreateMultiplexClient<TFeed>() where TFeed: KucoinBaseWebsocketFeed, new()
        {
            if (tunnelId != null && !isMultiplexHost)
            {
                throw new InvalidOperationException("Cannot initialize as multiplex connection host when already initialized as multiplex client.");
            }
            var client = new TFeed();

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

        #endregion

        #region Data Send and Receive

        private List<string> msgQueue;

        private Task keepAlive;

        private Task inputReaderThread;

        private Task msgPumpThread;


        /// <summary>
        /// The data receive thread.
        /// </summary>
        /// <returns></returns>
        private async Task DataReceiveThread()
        {

            byte[] inputChunk = new byte[chunkSize];

            StringBuilder sb = new StringBuilder();

            int strlen = 0;
            int level = 0;

            bool inQuote = false;
            bool inEsc = false;

            int i, c;

            sb.EnsureCapacity(recvBufferSize);

            var arrSeg = new ArraySegment<byte>(inputChunk);

            // loop forever or until the connection is broken or canceled.
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(arrSeg, ctsReceive.Token);
                c = result.Count;
                
                if (c == 0)
                {
                    // no data received.
                    // let's give up some time-slices and try again.
                    await Task.Delay(10);
                    continue;
                }

                strlen += c;

                if (sb.Capacity < strlen)
                {
                    sb.EnsureCapacity(strlen);
                }

                // process as many complete JSON objects as we can and
                // hold on to incomplete string data for the next
                // receive, which will complete the current object
                // and add additional objects, ad infinitum.
                for (i = 0; i < c; i++)
                {
                    // character by character is the simplest and fastest way.
                    char inChar = (char)inputChunk[i];
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

                            string json = sb.ToString();
                            sb.Clear();

                            strlen = 0;

                            // lock on message queue.
                            lock (msgQueue)
                            {
                                msgQueue.Add(json);
                            }

                            continue;
                        }
                    }
                }

            }
        }


        /// <summary>
        /// Separate thread that runs to pump messages to the observers in
        /// the order in which they were received without delaying the data
        /// receiving thread.
        /// </summary>
        /// <returns></returns>
        private async Task MessagePumpThread()
        {
            string[] queue;

            // loop forever
            while (!ctsReceive.IsCancellationRequested && socket?.State == WebSocketState.Open)
            {
                if (msgQueue.Count == 0)
                {
                    // nothing in the queue, give up some time-slices.
                    await Task.Delay(10);
                    continue;
                }

                // lock on msgQueue.
                lock (msgQueue)
                {
                    queue = msgQueue.ToArray();
                    msgQueue.Clear();
                }

                foreach (var s in queue)
                {
                    await RouteJsonPacket(s);
                }
            }
        }

        /// <summary>
        /// Handles initial processing and routing of JSON packets.
        /// </summary>
        /// <param name="json">The JSON string that was received from the remote endpoint.</param>
        /// <remarks>
        /// This method deserializes the JSON data and calls either the
        /// <see cref="OnPong(FeedMessage)"/> or <see cref="HandleMessage(FeedMessage)"/> methods, and the <see cref="OnJsonReceived(string)"/> method.
        /// OnJsonReceive is called after all other handling has occurred.
        /// </remarks>
        private async Task RouteJsonPacket(string json)
        {
            var e = JsonConvert.DeserializeObject<FeedMessage>(json);

            if (e.Type == "pong")
            {
                _ = Task.Run(() => OnPong(e));
            }
            else
            {
                if (tunnelId == e.TunnelId)
                {
                    await HandleMessage(e);
                }
                else if (isMultiplexHost)
                {
                    var p = multiplexClients.Where((child) => child.tunnelId == e.TunnelId).FirstOrDefault();

                    if (p != null)
                    {
                        await p.RouteJsonPacket(json);
                    }
                }
            }

            OnJsonReceived(json);
        }

        /// <summary>
        /// Called whenever a JSON data packet is received.
        /// </summary>
        /// <param name="json">The raw JSON string.</param>
        /// <remarks>
        /// The default behavior of this method is to fire the <see cref="DataReceived"/> event.
        /// </remarks>
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
        /// The default behavior of this method is to fire the <see cref="Pong"/> event.
        /// </remarks>
        protected virtual void OnPong(FeedMessage msg)
        {
            Pong?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Send binary data to the remote server.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task Send(byte[] data)
        {
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));

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
        /// <param name="text"></param>
        /// <param name="encoding">The optional <see cref="Encoding"/> to use to transmit the text.
        /// If this parameter is null, UTF-8 encoding is used, by default.
        /// </param>
        protected async Task Send(string text, Encoding encoding = null)
        {
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));
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
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));

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

        /// <summary>
        /// Ping the remote connection.
        /// </summary>
        /// <returns>A reply will come from the server as a 'pong' message.</returns>
        public async Task Ping()
        {
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));
            var e = new FeedMessage();

            e.Id = connectId.ToString("d");
            e.Type = "ping";

            await Send(e);
        }

        #endregion

        #region Thread cancellation

        /// <summary>
        /// Cancel all threads.
        /// </summary>
        protected void CancelAllThreads()
        {
            CancelReceiveThread();
            CancelSendThread();
            CancelPingThread();
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
        protected void CancelPingThread()
        {
            ctsPing?.Cancel();
        }

        #endregion

        #region IDisposable Pattern

        protected bool disposed = false;

        protected bool disposing = false;

        /// <summary>
        /// True if this object has been disposed.
        /// </summary>
        public bool Disposed => disposed;

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
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">True if being called from the <see cref="Dispose"/> method, false if being called by the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return; // throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));
            
            Disconnect();

            if (this.disposing) return;
            this.disposing = true;

            CancelAllThreads();

            if (isMultiplexHost && multiplexClients != null && multiplexClients.Count > 0)
            {
                // multiplexed children need to go, too.
                foreach (var child in multiplexClients)
                {
                    child.Dispose();
                }
            }

            socket?.Dispose();

            socket = null;
            token = null;
            tunnelId = null;

            disposed = true;

            if (disposing)
            {
                OnPropertyChanged(nameof(Connected));
                FeedDisconnected?.Invoke(this, new EventArgs());
            }
        }

        #endregion
    }

    /// <summary>
    /// Websocket feed abstract base class for <see cref="IObservable{T}"/> implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KucoinBaseWebsocketFeed<T> :
        KucoinBaseWebsocketFeed,
        IObservable<T>
        where T : class
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
        protected abstract string Subject { get; }

        /// <summary>
        /// Gets the topic for the feed subscription.
        /// </summary>
        protected abstract string Topic { get; }

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
        public KucoinBaseWebsocketFeed(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null) : base(key, secret, passphrase, isSandbox, url, isv1api, multiplexHost)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="credProvider"><see cref="ICredentialsProvider"/> implementation.</param>
        /// <param name="url">Optional alternate base URL.</param>
        /// <param name="isv1api">Is v1 API.</param>
        /// <param name="multiplexHost">Optional multiplex host to attach to as a client.</param>
        public KucoinBaseWebsocketFeed( 
            ICredentialsProvider credProvider,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexHost = null) : base(credProvider, url, isv1api, multiplexHost)
        {
        }

        #endregion

        #region IObservable<T> Pattern

        internal List<FeedObservation<T>> observations = new List<FeedObservation<T>>();

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

                var obsNew = new FeedObservation<T>(this, observer);
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
        internal virtual void RemoveObservation(FeedObservation<T> observation)
        {
            observation.Observer.OnCompleted();

            lock(observations)
            {
                if (observations.Contains(observation))
                {
                    observations.Remove(observation);
                }
            }

        }

        /// <summary>
        /// Push the deserialized subscription object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual async Task PushNext(T obj)
        {
            await Task.Run(() =>
            {
                List<Action> actions = new List<Action>();

                foreach (var obs in observations)
                {
                    actions.Add(() => obs.Observer.OnNext(obj));
                }

                Parallel.Invoke(actions.ToArray());

                if (FeedDataReceived != null)
                {
                    FeedDataReceived.Invoke(this, new FeedDataReceivedEventArgs<T>(obj));
                }

            });
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            socket?.Dispose();

            foreach (var observer in observations)
            {
                observer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
