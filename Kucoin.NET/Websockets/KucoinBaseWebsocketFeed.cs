
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kucoin.NET.Data.Market;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Kucoin.NET.Data.Websockets;

using System.Net.Http;
using System.Net.WebSockets;
using Kucoin.NET.Rest;
using System.Runtime.InteropServices;
using Kucoin.NET.Data.Interfaces;
using System.Security.Cryptography.X509Certificates;

namespace Kucoin.NET.Websockets
{
    /// <summary>
    /// Pong message event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PongHandler(object sender, EventArgs e);

    /// <summary>
    /// Data received event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DataReceivedHandler(object sender, DataReceivedEventArgs e);

    /// <summary>
    /// Data received event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DataReceivedHandler<T>(object sender, FeedDataReceivedEventArgs<T> e) where T: class;

    /// <summary>
    /// Feed connected event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FeedConnectedHandler(object sender, FeedConnectedEventArgs e);

    /// <summary>
    /// Feed disconnected event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FeedDisconnectedHandler(object sender, EventArgs e);

    /// <summary>
    /// Websocket feed abstract base class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KucoinBaseWebsocketFeed :
        KucoinBaseRestApi,
        IDisposable
    {
        #region Protected fields

        protected KucoinBaseWebsocketFeed multiplexParent;

        protected bool isMultiplexParent = false;

        protected List<KucoinBaseWebsocketFeed> multiplexChildren;

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
        public virtual event PongHandler Pong;

        /// <summary>
        /// Event that is raised when the client has established a connection with the server.
        /// </summary>
        public virtual event FeedConnectedHandler FeedConnected;

        /// <summary>
        /// Event that is raised when the client has disconnected from the server.
        /// </summary>
        /// <remarks>
        /// This event is raised when the object is manually disposed or disposed via the 'using' statement,
        /// but not when the destructor is called by the garbage collector.
        /// </remarks>
        public virtual event FeedDisconnectedHandler FeedDisconnected;

        /// <summary>
        /// Event that is raised for every single JSON entity that is received.
        /// </summary>
        /// <remarks>
        /// Avoid using, if possible, as calling this handler will have a performance impact.
        /// </remarks>
        public virtual event DataReceivedHandler DataReceived;

        #endregion

        #region Default Constructor

        public KucoinBaseWebsocketFeed(
            ICredentialsProvider credProvider,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexParent = null) 
            : base(
                  credProvider, 
                  isSandbox, 
                  url, 
                  isv1api
                  )
        {
            if (multiplexParent != null)
            {
                Task<bool> t = MultiplexInit(multiplexParent);
                t.Wait();

                if (t.Result != true)
                {
                    throw new InvalidOperationException("Failed to initialize multiplexer from parent.");
                }
            }
        }

        public KucoinBaseWebsocketFeed(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false, 
            KucoinBaseWebsocketFeed multiplexParent = null) 
            : base(
                  key, 
                  secret, 
                  passphrase, 
                  isSandbox, 
                  url, 
                  isv1api
                  )
        {
            if (multiplexParent != null)
            {
                Task<bool> t = MultiplexInit(multiplexParent);
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
                if (multiplexParent != null)
                {
                    return multiplexParent.Connected;
                }
                else if (socket != null && socket.State == WebSocketState.Open)
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
        /// Gets the server associated with this instance.
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
        public KucoinBaseWebsocketFeed MultiplexParent => multiplexParent;

        /// <summary>
        /// True if this object is the connection holder (parent) of a multiplexed connection.
        /// </summary>
        public bool IsMultiplexParent => isMultiplexParent;

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
            socket.Dispose();
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
        /// <param name="initAsMultiplexParent">True to initialize as a multiplexing tunnel parent.</param>
        /// <param name="tunnelId">Optional tunnel id for initialization.  If one is not specified, one will be created.</param>
        /// <returns>True if the connection was negotiated and established, successfully.</returns>
        public async Task<bool> Connect(bool initAsMultiplexParent = false, string tunnelId = null)
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

                keepAlive = Task.Run(async () =>
                {
                    while (!ctsPing.IsCancellationRequested && Connected)
                    {
                        await Task.Delay(server.PingInterval);
                        await Ping();
                    }

                }, ctsPing.Token);

                // The Reader: 
                // -----------

                // message queue
                msgQueue = new List<string>();

                // data receiver
                inputReaderThread = Task.Run(() => DataReceiveThread(), ctsReceive.Token);

                // observer notification pump
                msgPumpThread = Task.Run(() => MessagePumpThread(), ctsReceive.Token);

                OnPropertyChanged(nameof(Connected));
                FeedConnected?.Invoke(this, new FeedConnectedEventArgs(connectId, server, token.Data.Token));

                if (initAsMultiplexParent)
                {
                    await MultiplexInit(tunnelId);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Multiplexing

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

        public virtual async Task<bool> RemoveMultiplexChannels()
        {
            if (isMultiplexParent)
            {
                foreach (var child in multiplexChildren)
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
        /// Initialize multiplexing as the parent.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> MultiplexInit(string newTunnelId = null)
        {
            if (!Connected || isMultiplexParent || multiplexParent != null) return false;

            isMultiplexParent = true;
            tunnelId = newTunnelId ?? Guid.NewGuid().ToString("d");
            multiplexChildren = new List<KucoinBaseWebsocketFeed>();

            await OpenMultiplexChannel(tunnelId);

            return true;
        }

        /// <summary>
        /// Create a multiplexed child of the specified type to share the connection of this <see cref="KucoinBaseWebsocketFeed"/>-derived instance.
        /// </summary>
        /// <typeparam name="F">The feed type to create</typeparam>
        /// <returns>A new feed that shares a connection with the current object.</returns>
        public virtual async Task<F> CreateMultiplexedChild<F>() where F: KucoinBaseWebsocketFeed, new()
        {
            if (tunnelId != null && !isMultiplexParent)
            {
                throw new InvalidOperationException("Cannot initialize as multiplex parent when already initialized as multiplex child.");
            }
            var child = new F();

            if (!Connected)
            {
                await Connect(true);
            }
            else if (!isMultiplexParent)
            {
                await MultiplexInit();
            }

            await child.MultiplexInit(this);
            return child;
        }

        /// <summary>
        /// Initialize multiplexing as a child.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual async Task<bool> MultiplexInit(KucoinBaseWebsocketFeed parent, string newTunnelId = null)
        {
            if (Connected || tunnelId != null)
            {
                return false;
            }

            tunnelId = newTunnelId ?? Guid.NewGuid().ToString("d");
            multiplexParent = parent;

            if (parent.multiplexChildren == null)
            {
                if (!await parent.MultiplexInit()) return false;
            }
            else if (parent.multiplexChildren.Contains(this))
            {
                return false;
            }
            else if (parent.multiplexChildren.Count >= 4)
            {
                return false;
            }

            parent.multiplexChildren.Add(this);
            await parent.OpenMultiplexChannel(tunnelId);

            return true;
        }

        #endregion

        #region Data Send and Receive

        private List<string> msgQueue;

        private Task keepAlive;

        private Task inputReaderThread;

        private Task msgPumpThread;

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
            while (!ctsReceive.IsCancellationRequested && socket != null && socket?.State == WebSocketState.Open)
            {
                await Task.Delay(10);

                if (msgQueue.Count == 0)
                {
                    continue;
                }

                lock (msgQueue)
                {
                    queue = msgQueue.ToArray();
                    msgQueue.Clear();
                }

                foreach (var s in queue)
                {
                    await InternalJsonReceive(s);
                }
            }
        }

        protected virtual void OnJsonReceived(string json)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(json));
        }

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
            while (!ctsReceive.IsCancellationRequested && socket != null && socket.State == WebSocketState.Open)
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

                            // lock the message queue to append it.
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
        /// Handles receiving of JSON data.
        /// </summary>
        /// <param name="json">The JSON string that was received from the remote endpoint.</param>
        /// <remarks>
        /// Normally this method will deserialize the JSON data and call either the
        /// <see cref="HandlePong(FeedMessage)"/> or <see cref="HandleMessage(FeedMessage)"/> methods.
        /// If you wish to override this method, those methods will not automatically be called unless
        /// you call the base method.
        /// </remarks>
        protected virtual async Task InternalJsonReceive(string json)
        {
            var e = JsonConvert.DeserializeObject<FeedMessage>(json);

            if (e.Type == "pong")
            {
                await HandlePong(e);
            }
            else
            {
                if (tunnelId == e.TunnelId)
                {
                    await HandleMessage(e);
                }
                else if (isMultiplexParent)
                {
                    foreach (var child in multiplexChildren)
                    {
                        if (child.tunnelId == e.TunnelId)
                        {
                            await child.InternalJsonReceive(json);
                            break;
                        }
                    }
                }
            }

            OnJsonReceived(json);
        }

        /// <summary>
        /// Handle a pong (a response to a ping).
        /// </summary>
        /// <param name="msg">The contents of the pong message.</param>
        /// <remarks>
        /// You may override this function to do periodic work in your own application.
        /// </remarks>
        protected virtual async Task HandlePong(FeedMessage msg)
        {
            if (Pong != null)
            {
                await Task.Run(() => Pong?.Invoke(this, new EventArgs()));
            }
        }

        /// <summary>
        /// Send binary data to the remote server.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task Send(byte[] data)
        {
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));

            if (multiplexParent != null)
            {
                await multiplexParent.socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, multiplexParent.ctsSend.Token);
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
            if (disposed) throw new ObjectDisposedException(nameof(KucoinBaseWebsocketFeed));

            if (this.disposing) return;
            this.disposing = true;

            CancelAllThreads();

            if (isMultiplexParent && multiplexChildren != null && multiplexChildren.Count > 0)
            {
                // multiplexed children need to go, too.
                foreach (var child in multiplexChildren)
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
        public virtual event DataReceivedHandler<T> FeedDataReceived;

        /// <summary>
        /// Gets the message subject for the feed cycle.
        /// </summary>
        protected abstract string Subject { get; }

        /// <summary>
        /// Gets the topic for the feed subscription.
        /// </summary>
        protected abstract string Topic { get; }

        #region Default Constructor 

        public KucoinBaseWebsocketFeed(
            string key,
            string secret,
            string passphrase,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexParent = null) : base(key, secret, passphrase, isSandbox, url, isv1api, multiplexParent)
        {
        }


        public KucoinBaseWebsocketFeed( 
            ICredentialsProvider credProvider,
            bool isSandbox = false,
            string url = null,
            bool isv1api = false,
            KucoinBaseWebsocketFeed multiplexParent = null) : base(credProvider, isSandbox, url, isv1api, multiplexParent)
        {
        }

        #endregion

        #region IObservable<T> Pattern

        internal List<FeedObservation<T>> observers = new List<FeedObservation<T>>();

        /// <summary>
        /// Subscribe to this feed.
        /// </summary>
        /// <param name="observer">A class object that implements the <see cref="IObserver{T}"/> interface.</param>
        /// <returns>An <see cref="IDisposable"/> implementation that can be used to cancel the subscription.</returns>
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observers)
            {
                foreach (var obs in observers)
                {
                    if (obs.Observer == observer) return obs;
                }

                var obsNew = new FeedObservation<T>(this, observer);
                observers.Add(obsNew);

                return obsNew;
            }

        }

        /// <summary>
        /// Push the object to the observers.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual async Task PushNext(T obj)
        {
            await Task.Run(() =>
            {

                foreach (var obs in observers)
                {
                    obs.Observer.OnNext(obj);
                }

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

            foreach (var observer in observers)
            {
                observer.Dispose();
            }

            base.Dispose(disposing);

        }
    }



}
