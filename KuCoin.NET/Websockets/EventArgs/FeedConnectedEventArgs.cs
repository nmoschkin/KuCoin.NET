using KuCoin.NET.Data.Websockets;

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace KuCoin.NET.Websockets
{
    /// <summary>
    /// Feed IsConnected event arguments
    /// </summary>
    public class FeedConnectedEventArgs : EventArgs
    {

        /// <summary>
        /// Gets the token for the connection.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Information about the server connection.
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        /// The connection Id
        /// </summary>
        public Guid ConnectionId { get; private set; }

        public FeedConnectedEventArgs(Guid connectId, Server server, string token)
        {
            ConnectionId = connectId;
            Server = server;
            Token = token;
        }
    }


    public class FeedDisconnectedEventArgs : EventArgs
    {
        public WebSocketCloseStatus CloseStatus { get; private set; }

        public FeedDisconnectedEventArgs(WebSocketCloseStatus status)
        {
            CloseStatus = status;
        }
    }

}
