using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Helpers;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using KuCoin.NET.Websockets.Distribution;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;
using System.Collections.ObjectModel;

namespace KuCoin.NET.Websockets.Observations
{

    /// <summary>
    /// Level 3 Market Feed Order Book.
    /// </summary>
    public sealed class Level3OrderBook : OrderBookDistributable<KeyedAtomicOrderBook<AtomicOrderUnit>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3Update, Level3>
    {

        /// <summary>
        /// Instantiate a new level 3 order book.
        /// </summary>
        /// <param name="parent">The parent <see cref="Level3"/> or <see cref="Level3Direct"/> feed.</param>
        /// <param name="symbol">The market trading symbol this order book is tracking.</param>
        /// <param name="direct">True if the feed is direct, otherwise false.</param>
        public Level3OrderBook(Level3 parent, string symbol, bool direct) : base(parent, symbol, true, direct)
        {
            this.parent = parent;
            this.direct = direct;
            dataProvider = parent;
            IsPresentationDisabled = true;
        }

        /// <summary>
        /// Instantiate a new level 3 order book.
        /// </summary>
        /// <param name="parent">The parent <see cref="Level3"/> or <see cref="Level3Direct"/> feed.</param>
        /// <param name="symbol">The market trading symbol this order book is tracking.</param>
        public Level3OrderBook(Level3 parent, string symbol) : base(parent, symbol, true, parent is Level3Direct)
        {
            this.parent = parent;
            dataProvider = parent;
            IsPresentationDisabled = true;
        }

        public override void OnNext(Level3Update value)
        {
            if (!direct)
            {
                base.OnNext(value);
            }
            else
            {
                lock (lockObj)
                {
                    buffer.Add(value);
                }

                if (initialized) DoWork();
                else Thread.Sleep(0);

                if (diagEnable)
                {
                    if (DateTime.UtcNow.Ticks - marktime >= 10_000_000)
                    {
                        marktime = DateTime.UtcNow.Ticks;
                        throughput = bytesreceived * 8;
                        bytesreceived = 0;
                    }
                    else
                    {
                        bytesreceived += value.size;
                    }
                }

            }
        }

        long bytesreceived;
        long throughput;
        long marktime = DateTime.UtcNow.Ticks;

        public override bool DiagnosticsEnabled 
        {
            get => base.DiagnosticsEnabled; 
            set
            {
                if (diagEnable != value)
                {
                    base.DiagnosticsEnabled = value;

                    bytesreceived = 0;
                    throughput = 0;
                    marktime = 0;

                    if (fullDepth != null)
                    {
                        fullDepth.Asks.EnableMetrics = value;
                        fullDepth.Bids.EnableMetrics = value;
                    }
                }
            }
        }

        public long Throughput
        {
            get => throughput;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            lock (lockObj)
            {
                if (fullDepth != null)
                {
                    fullDepth.Asks.EnableMetrics = diagEnable;
                    fullDepth.Bids.EnableMetrics = diagEnable;

                    fullDepth.Asks.TryRebalance();
                    fullDepth.Bids.TryRebalance();

                    if (diagEnable)
                    {
                        fullDepth.Asks.ResetMetrics();
                        fullDepth.Bids.ResetMetrics();
                    }

                    parent.Logger.Log($"{Symbol} Initialized. Market Depth: {fullDepth.Asks.Count:#,##0} Asks, {fullDepth.Bids.Count:#,##0} Bids.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyObservable(ICollection<AtomicOrderUnit> src, ObservableCollection<ObservableAtomicOrderUnit> dest)
        {
            int md = this.marketDepth;

            if (src.Count < md) md = src.Count;

            if (dest.Count != md)
            {
                dest.Clear();

                int i = 0;
                foreach (var item in src)
                {
                    dest.Add(item.Clone<ObservableAtomicOrderUnit>());
                    i++;
                    if (i >= md) break;
                }
            }
            else
            {
                int i = 0;
                foreach (var item in src)
                {
                    if (dest[i].OrderId == item.OrderId)
                    {
                        dest[i].Price = item.Price;
                        dest[i].Size = item.Size;
                        dest[i].Timestamp = item.Timestamp;
                        dest[i].Sequence = item.Sequence;
                    }
                    else
                    {
                        dest[i] = item.Clone<ObservableAtomicOrderUnit>();
                    }

                    i++;
                    if (i >= md) break;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void PresentData()
        {
            lock (lockObj)
            {
                if (fullDepth == null) return;
                if (this.marketDepth <= 0) return;

                var asks = fullDepth.Asks;
                var bids = fullDepth.Asks;

                CopyObservable(asks, orderBook.Asks);
                CopyObservable(bids, orderBook.Bids);

            }
        }
        
        bool rose = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(Level3Update obj)
        {
            if (disposedValue) return false;

            lock (lockObj)
            {
                if (fullDepth == null)
                {
                    if (!failure)
                    {
                        Failure = true;
                        FailReason = FailReason.Other;

                    }
                    return true;
                }
                else if (obj.Sequence <= fullDepth.Sequence)
                {
                    return true;
                }
                else if (obj.Sequence - fullDepth.Sequence > 1)
                {
                    if (!rose)
                    {
                        parent.Logger.Log($"{Symbol} is resetting because the order book is out of sequence at {fullDepth.Sequence} versus incoming sequence {obj.Sequence}.");
                        rose = true;
                    }

                    _ = Reset();
                    return false;
                }

                if (diagEnable)
                {
                    var tt = DateTime.UtcNow.Ticks;
                    if (tt - matchTime >= 10_000_000)
                    {
                        matchTime = tt;

                        TransactionsPerSecond = transactSec;
                        MatchesPerSecond = matchSec;
                        transactSec = 0;
                        matchSec = 0;
                    }

                    GrandTotal++;
                    transactSec++;

                    if (obj.sc == 'm')
                    {
                        MatchTotal++;
                        matchSec++;
                    }
                }
            
                if (obj.Side == null)
                {
                    if (obj.sc == 'd')
                    {
                        fullDepth.Asks.Remove(obj.OrderId);
                        fullDepth.Bids.Remove(obj.OrderId);
                    }
                }
                else if (obj.Side == Side.Sell)
                {
                    if (!SequencePieces(obj, fullDepth.Asks, fullDepth.Bids))
                    {
                        parent.Logger.Log($"{Symbol} is resetting because of a bad Sell packet.");
                        rose = true;
                        _ = Reset();
                        return false;
                    }
                }
                else if (obj.Side == Side.Buy)
                {
                    if (!SequencePieces(obj, fullDepth.Bids, fullDepth.Asks))
                    {
                        parent.Logger.Log($"{Symbol} is resetting because of a bad Buy packet.");
                        rose = true;
                        _ = Reset();
                        return false;
                    }
                }

                fullDepth.Sequence = obj.Sequence;
                fullDepth.Timestamp = obj.Timestamp ?? DateTime.Now;

                if (updVol)
                {
                    decimal price = (decimal)fullDepth.Bids.First.Price;

                    if (!Candle.IsTimeInCandle(candle, fullDepth.Timestamp))
                    {
                        LastCandles.Add(candle);

                        Candle = new Candle();

                        Candle.Type = (KlineType)KlineType;
                        Candle.Volume = 0;

                        Candle.OpenPrice = Candle.ClosePrice = Candle.HighPrice = Candle.LowPrice = price;
                        KlineTime = Candle.Timestamp = klineType.GetCurrentKlineStartTime();
                    }
                    else
                    {
                        Candle.ClosePrice = price;

                        if (price > Candle.HighPrice)
                        {
                            Candle.HighPrice = price;
                        }
                        else if (price < Candle.LowPrice)
                        {
                            Candle.LowPrice = price;    
                        }
                    }

                }

                if (rose) rose = false;
                return true;
            }
        }
                
        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SequencePieces(Level3Update change, KeyedBook<AtomicOrderUnit> pieces, KeyedBook<AtomicOrderUnit> otherPieces)
        {
            switch (change.sc)
            {
                case 'd':

                    pieces.Remove(change.OrderId);
                    return true;

                case 'o':

                    if (change.Price == null || change.Price == 0 || change.Size == null || change.Size == 0) return true;

                    var u = new AtomicOrderUnit
                    {
                        Price = change.Price ?? 0,
                        Size = change.Size ?? 0,
                        Timestamp = change.Timestamp ?? DateTime.Now,
                        OrderId = change.OrderId,
                        Sequence = change.Sequence
                    };

                    if (pieces.ContainsKey(u.OrderId))
                    {
                        var pfail = pieces[u.OrderId];  
                        
                        if (pfail.Size == 0)
                        {
                            parent.Logger.Log($"{Symbol} {u.OrderId} already exists but is zero. Replacing.");
                            pieces.Remove(pfail);
                        }
                        else
                        {
                            parent.Logger.Log($"{Symbol} {u.OrderId} already exists!");
                            parent.Logger.Log($"{Symbol} Existing Order: {pfail}");
                            parent.Logger.Log($"{Symbol} New Order:      {u}");

                            return false;
                        }
                    }

                    pieces.Add(u);
                    return true;

                case 'c':

                    if (pieces.TryGetValue(change.OrderId, out AtomicOrderUnit piece))
                    {
                        if (pieces.TryAlterItem(piece, (p) =>
                        {
                            p.Size = change.Size ?? 0;
                            p.Timestamp = change.Timestamp ?? DateTime.Now;
                            p.Price = change.Price ?? 0;
                            p.Sequence = change.Sequence;

                            return p;
                        }))
                        {
                            return true;
                        }
                    }

                    parent.Logger.Log($"{Symbol} Change {change.OrderId} did not succeed.");
                    return false;

                case 'm':

                    if (change.Price is decimal p && change.Size is decimal csize
                        && otherPieces.TryGetValue(change.MakerOrderId, out AtomicOrderUnit o))
                    {

                        if (!otherPieces.TryAlterItem(o, (itm) => {
                            itm.Size -= csize;
                            itm.Sequence = change.Sequence;
                            return itm;
                        }))
                        {
                            parent.Logger.Log($"{Symbol} Match {change.OrderId} did not succeed.");
                            return false;
                        }

                        // A match is a real component of volume.
                        // we can keep our own tally of the market volume per k-line.
                        if (updVol)
                        {
                            var d = (csize * p);
                            Candle.Volume += d;
                        }
                    }

                    return true;

            }

            return false;
        }

    }

}
