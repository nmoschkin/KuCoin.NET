using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using KuCoin.NET.Websockets.Distribution.Services;
using System.ComponentModel;

namespace KuCoin.NET.Websockets.Distribution
{
    /// <summary>
    /// An object that can ping a remote server, and be registered with the <see cref="PingService"/>.
    /// </summary>
    public interface IPingable : INotifyPropertyChanged
    {
        /// <summary>
        /// Ping the remote server.
        /// </summary>
        void Ping();        

        /// <summary>
        /// The ping interval (in milliseconds.)
        /// </summary>
        /// <remarks>
        /// This number should be evenly divisible by 10.
        /// </remarks>
        int PingInterval { get; set; }

    }

    /// <summary>
    /// An object that can ping a remote server asynchronously, and be registered with the <see cref="PingService"/>.
    /// </summary>
    public interface IAsyncPingable : IPingable
    {
        /// <summary>
        /// Ping the remote server asynchronously.
        /// </summary>
        /// <returns></returns>
        new Task Ping();
    }
}
