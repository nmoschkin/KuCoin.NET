﻿using Kucoin.NET.Data.Market;
using Kucoin.NET.Data.Websockets;
using Kucoin.NET.Helpers;
using Kucoin.NET.Websockets.Observations;
using Kucoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Services
{

    /// <summary>
    /// Represents the data service for a trading symbol.
    /// </summary>
    public interface ISymbolDataService : ISymbol, IDisposable, INotifyPropertyChanged
    {

        /// <summary>
        /// Gets detailed information about the trading symbol.
        /// </summary>
        TradingSymbol TradingSymbolInfo { get; }

        /// <summary>
        /// Gets the level 2 observation, or null if not active.
        /// </summary>
        Level2Observation Level2Observation { get; }

        /// <summary>
        /// Gets the level 2 feed, or null if not active.
        /// </summary>
        Level2 Level2Feed { get; }

        /// <summary>
        /// Gets the level 3 observation, or null if not active.
        /// </summary>
        Level3Observation Level3Observation { get; }

        /// <summary>
        /// Gets the level 2 static depth market feed (5 best ask/bid), or null if not active.
        /// </summary>
        Level2Depth5 Level2Depth5 { get; }

        /// <summary>
        /// Gets the level 2 static depth market feed (50 best ask/bid), or null if not active.
        /// </summary>
        Level2Depth50 Level2Depth50 { get; }

        /// <summary>
        /// The observable market depth 5 list.
        /// </summary>
        ObservableStaticMarketDepthUpdate Level2Depth5Update { get; }

        /// <summary>
        /// The observable market depth 50 list.
        /// </summary>
        ObservableStaticMarketDepthUpdate Level2Depth50Update { get; }

        /// <summary>
        /// Gets the level 3 feed, or null if not active.
        /// </summary>
        Level3 Level3Feed { get; }    

        /// <summary>
        /// Gets a value that indicates that the service is connected.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Establish a connection to the remote system using a credentials provider.
        /// </summary>
        /// <param name="credentialsProvider">The credentials provider.</param>
        /// <remarks>
        /// <paramref name="credentialsProvider"/> can be null, but level 2 and level 3 feeds will be unavailable.
        /// </remarks>
        Task<bool> Connect(ICredentialsProvider credentialsProvider);


        /// <summary>
        /// Attempt to drop and re-establish the connection.
        /// </summary>
        /// <param name="flushSubscriptions">True to reinstantiate all classes and dispose subscribed observers.</param>
        /// <returns></returns>
        Task<bool> Reconnect(bool flushSubscriptions);

        /// <summary>
        /// Subscribe to the Kline
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns></returns>
        IDisposable SubscribeKline(KlineType klineType, IObserver<KlineFeedMessage<Candle>> observer);

        /// <summary>
        /// Subscribe to the ticker
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns></returns>
        IDisposable SubscribeTicker(IObserver<Ticker> observer);
        /// <summary>
        /// Enable level 3
        /// </summary>
        Task EnableLevel2();

        /// <summary>
        /// Enable level 2
        /// </summary>
        Task EnableLevel3();

        /// <summary>
        /// Gets the 24-hour stats
        /// </summary>
        /// <returns></returns>
        Task<AllSymbolsTickerItem> Get24HourStats();

        /// <summary>
        /// Gets the most recent K-Line.
        /// </summary>
        /// <typeparam name="TCandle">The type of concrete candle object to create.  Must implmeent <see cref="IWritableCandle"/>.</typeparam>
        /// <typeparam name="TCol">The type of collection to return.  Must implement <see cref="IList{T}"/> of <typeparamref name="TCustom"/>.</typeparam>
        /// <typeparam name="TCustom">The item type for the collection that is returned.</typeparam>
        /// <param name="klineType">The type of K-Line to retrieve.</param>
        /// <param name="pieces">The number of pieces (max 200)</param>
        /// <returns></returns>
        Task<TCol> GetKline<TCandle, TCustom, TCol>(KlineType klineType, int pieces)
            where TCandle : IWritableCandle, TCustom, new()
            where TCol : IList<TCustom>, new();


        /// <summary>
        /// Gets the current price ticker.
        /// </summary>
        /// <returns></returns>
        Task<Ticker> GetTicker();

        /// <summary>
        /// Subscribe to one of the level 2 static market depth feeds (either 5 or 50 best asks/bids)
        /// </summary>
        /// <param name="depth">The depth of the market.</param>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns></returns>
        Task<ObservableStaticMarketDepthUpdate> SubscribeLevel2StaticDepth(Level2Depth depth, IObserver<StaticMarketDepthUpdate> observer);

        /// <summary>
        /// Activate a new symbol. The resources for the previous symbol are released, and the same active services are restored for the new symbol.
        /// </summary>
        /// <param name="newSymbol">The new symbol to subscribe to.</param>
        /// <returns></returns>
        Task<string> ChangeSymbol(string newSymbol);

        /// <summary>
        /// Clear the symbol and drop all external subscriptions (the connection does not get ended)
        /// </summary>
        /// <returns></returns>
        Task ClearSymbol();

        /// <summary>
        /// Return a new data service for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get the data service for.</param>
        /// <param name="shareConnection">True to share the current instance's connections with the new service.</param>
        /// <returns></returns>
        Task<ISymbolDataService> AddSymbol(string symbol, bool shareConnection);

    }
}