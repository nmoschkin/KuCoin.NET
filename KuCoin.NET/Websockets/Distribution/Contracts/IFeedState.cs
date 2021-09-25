using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An object that has a <see cref="FeedState"/>.
    /// </summary>
    public interface IFeedState
    {  
        /// <summary>
        /// Gets the current state of the feed.
        /// </summary>
        FeedState State { get; }

        /// <summary>
        /// Refresh the current state of the feed.
        /// </summary>
        void RefreshState();
    }
}
