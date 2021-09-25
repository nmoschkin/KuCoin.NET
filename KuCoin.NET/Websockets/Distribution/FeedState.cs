using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// Enumeration values that indicate the current state of a websocket feed, or listener.
    /// </summary>
    public enum FeedState
    {
        /// <summary>
        /// The feed is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// The feed is connected, but not subscribed
        /// </summary>
        Connected,

        /// <summary>
        /// The feed is subscribed but not running.
        /// </summary>
        Subscribed,

        /// <summary>
        /// The feed has been unsubscribed.
        /// </summary>
        Unsubscribed,

        /// <summary>
        /// The feed is loading or initializing.
        /// </summary>
        Initializing,

        /// <summary>
        /// The feed is running.
        /// </summary>
        Running,

        /// <summary>
        /// The feed is not operating normally.
        /// </summary>
        Failed,

        /// <summary>
        /// The feed is in multiple states.  Query each listener for the current state.
        /// </summary>
        Multiple

    }
}
