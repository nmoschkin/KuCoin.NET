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

namespace KuCoin.NET.Websockets.Observations
{
    public sealed class Level3OrderBook : OrderBookDistributable<KeyedAtomicOrderBook<AtomicOrderUnit>, ObservableAtomicOrderBook<ObservableAtomicOrderUnit>, Level3Update, Level3>
    {

        public Level3OrderBook(Level3 parent, string symbol, bool direct) : base(parent, symbol, true, direct)
        {
            this.parent = parent;
            this.direct = direct;
            dataProvider = parent;
            IsPresentationDisabled = true;
        }

        public Level3OrderBook(Level3 parent, string symbol) : base(parent, symbol, true, false)
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
        public long Throughput
        {
            get => throughput;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void PresentData()
        {
            lock (lockObj)
            {
                if (fullDepth == null) return;
                if (this.marketDepth <= 0) return;

                var asks = fullDepth.Asks as IList<AtomicOrderUnit>;
                var bids = fullDepth.Asks as IList<AtomicOrderUnit>;

                if (orderBook == null)
                {
                    OrderBook = new ObservableAtomicOrderBook<ObservableAtomicOrderUnit>();
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;
                }

                int marketDepth = this.marketDepth;

                if (asks.Count < marketDepth) marketDepth = asks.Count;

                if (orderBook.Asks.Count != marketDepth)
                {
                    orderBook.Asks.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Asks.Add(asks[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Asks[i].Price == asks[i].Price)
                        {
                            orderBook.Asks[i].Price = asks[i].Price;
                            orderBook.Asks[i].Size = asks[i].Size;
                        }
                        else
                        {
                            orderBook.Asks[i] = asks[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

                if (bids.Count < marketDepth) marketDepth = bids.Count;

                if (orderBook.Bids.Count != marketDepth)
                {
                    orderBook.Bids.Clear();
                    for (int i = 0; i < marketDepth; i++)
                    {
                        orderBook.Bids.Add(bids[i].Clone<ObservableAtomicOrderUnit>());
                    }
                }
                else
                {
                    for (int i = 0; i < marketDepth; i++)
                    {
                        if (orderBook.Bids[i].Price == bids[i].Price)
                        {
                            orderBook.Bids[i].Price = bids[i].Price;
                            orderBook.Bids[i].Size = bids[i].Size;
                        }
                        else
                        {
                            orderBook.Bids[i] = bids[i].Clone<ObservableAtomicOrderUnit>();
                        }
                    }
                }

            }

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(Level3Update obj)
        {
            if (disposedValue) return false;

            lock (lockObj)
            {
                if (fullDepth == null)
                {
                    if (!failure) Failure = true;
                    return false;
                }
                else if (obj.Sequence <= fullDepth.Sequence)
                {
                    return false;
                }
                else if (obj.Sequence - fullDepth.Sequence > 1)
                {
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
                        if (fullDepth.Asks.ContainsKey(obj.OrderId))
                        {
                            fullDepth.Asks.Remove(obj.OrderId);
                        }

                        if (fullDepth.Bids.ContainsKey(obj.OrderId))
                        {
                            fullDepth.Bids.Remove(obj.OrderId);
                        }
                    }
                }
                else if (obj.Side == Side.Sell)
                {
                    SequencePieces(obj.sc, obj, fullDepth.Asks, fullDepth.Bids);
                }
                else if (obj.Side == Side.Buy)
                {
                    SequencePieces(obj.sc, obj, fullDepth.Bids, fullDepth.Asks);
                }

                fullDepth.Sequence = obj.Sequence;
                fullDepth.Timestamp = obj.Timestamp ?? DateTime.Now;

                if (updVol)
                {
                    decimal price = (decimal)fullDepth.Bids[0].Price;

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

                return true;
            }
        }
                
        /// <summary>
        /// Sequence the changes into the order book.
        /// </summary>
        /// <param name="changes">The changes to sequence.</param>
        /// <param name="pieces">The collection to change (either an ask or a bid collection)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SequencePieces(char subj, Level3Update change, KeyedBook<AtomicOrderUnit> pieces, KeyedBook<AtomicOrderUnit> otherPieces)
        {
            switch (subj)
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
                        OrderId = change.OrderId
                    };

                    if (pieces.ContainsKey(u.OrderId))
                    {
                        return false;
                    }

                    pieces.Add(u);

                    return true;

                case 'c':

                    if (pieces.TryGetValue(change.OrderId, out AtomicOrderUnit piece))
                    {
                        pieces.Remove(piece.OrderId);

                        piece.Size = change.Size ?? 0;
                        piece.Timestamp = change.Timestamp ?? DateTime.Now;
                        piece.Price = change.Price ?? 0;

                        pieces.Add(piece);
                    }
                    else
                    {
                        return false;
                    }

                    return true;

                case 'm':

                    if (change.Price is decimal p && change.Size is decimal csize
                        && otherPieces.TryGetValue(change.MakerOrderId, out AtomicOrderUnit o))
                    {
                        o.Size -= csize;

                        otherPieces.Remove(o.OrderId);
                        otherPieces.Add(o);

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
