using KuCoin.NET.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Distribution
{
    
    /// <summary>
    /// An object that listens to websocket feeds.
    /// </summary>
    public interface IWebsocketListener : IParent, IDisposable
    {
    }

    /// <summary>
    /// An object that listens to a websocket feed streaming <typeparamref name="T"/> objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWebsocketListener<T> : IWebsocketListener where T: IStreamableObject
    {
    }
}
