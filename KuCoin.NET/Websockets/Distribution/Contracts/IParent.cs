using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{

    /// <summary>
    /// Provides an interface for an object that has a distributor parent.
    /// </summary>
    public interface IParent
    {
        /// <summary>
        /// Gets the parent distributor
        /// </summary>
        IWebsocketFeed Parent { get; }
    }

    /// <summary>
    /// Provides an interface for an object that has a distributor parent.
    /// </summary>
    /// <typeparam name="T">The type of the parent.</typeparam>
    public interface IParent<T> : IParent where T: IWebsocketFeed
    {
        /// <summary>
        /// Gets the parent distributor
        /// </summary>
        new T Parent { get; }
    }

}
