using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Websockets;
using KuCoin.NET.Websockets.Distribution;
using KuCoin.NET.Websockets.Public;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Observations
{
    public class Level2OrderBook : OrderBookDistributable<AggregatedOrderBook<OrderUnit>, ObservableOrderBook<ObservableOrderUnit>, Level2Update, Level2>, IObserver<MatchExecution>
    {
        int cresets = 0;
        DateTime lreset = DateTime.Now;

        public Level2OrderBook(Level2 parent, string symbol) : base(parent, symbol, false, parent is Level2Direct)
        {
            this.parent = parent;
            base.parent = parent;
            this.dataProvider = parent;
            this.IsPresentationDisabled = true;
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


        public override void OnNext(Level2Update value)
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

        protected override void PerformResetTasks()
        {
            base.PerformResetTasks();

            cresets++;

            if (cresets > 10 && (lreset - DateTime.Now) < new TimeSpan(0, 0, 0, 30))
            {
                cresets = 0;

                Failure = true;
            }

            lreset = DateTime.Now;
        }

        private void CopyObservable(ICollection<OrderUnit> src, ObservableCollection<ObservableOrderUnit> dest)
        {
            int md = this.marketDepth;

            if (src.Count < md) md = src.Count;

            if (dest.Count != md)
            {
                dest.Clear();

                int i = 0;
                foreach (var item in src)
                {
                    dest.Add(item.Clone<ObservableOrderUnit>());
                    i++;
                    if (i >= md) break;
                }
            }
            else
            {
                int i = 0;
                foreach (var item in src)
                {
                    if (dest[i].Price == item.Price)
                    {
                        dest[i].Size = item.Size;
                    }
                    else
                    {
                        dest[i] = item.Clone<ObservableOrderUnit>();
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

                var asks = fullDepth.Asks as ICollection<OrderUnit>;
                var bids = fullDepth.Bids as ICollection<OrderUnit>;

                if (orderBook == null)
                {
                    OrderBook = new ObservableOrderBook<ObservableOrderUnit>();
                    orderBook.Sequence = fullDepth.Sequence;
                    orderBook.Timestamp = fullDepth.Timestamp;
                }

                CopyObservable(asks, orderBook.Asks);
                CopyObservable(bids, orderBook.Bids);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ProcessObject(Level2Update obj)
        {
            if (disposedValue) return false;
    
            lock (lockObj)
            {
                if (fullDepth == null)
                {
                    if (!failure) Failure = true;
                    return true;
                }
                else if (obj.SequenceEnd <= fullDepth.Sequence)
                {
                    return true;
                }
                else if (obj.SequenceStart - fullDepth.Sequence > 1)
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
                }


                SequencePieces(obj.Changes.Asks, fullDepth.Asks);
                SequencePieces(obj.Changes.Bids, fullDepth.Bids);

                fullDepth.Sequence = obj.SequenceEnd;
                fullDepth.Timestamp = DateTime.Now;

                if (updVol)
                {
                    decimal price = fullDepth.Bids.FirstOrDefault().Price;

                    if (!Candle.IsTimeInCandle(candle, fullDepth.Timestamp.ToUniversalTime()))
                    {
                        candle.ClosePrice = price;
                        LastCandles.Add(candle);

                        Candle = new Candle() { Type = (KlineType)klineType, Timestamp = klineTime };
                        candle.Volume = 0;

                        candle.OpenPrice = candle.ClosePrice = candle.HighPrice = candle.LowPrice = price;
                        KlineTime = candle.Timestamp = klineType.GetCurrentKlineStartTime();
                    }
                    else
                    {
                        candle.ClosePrice = price;

                        if (price > candle.HighPrice)
                        {
                            candle.HighPrice = price;
                        }
                        else if (price < candle.LowPrice)
                        {
                            candle.LowPrice = price;
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
        protected void SequencePieces(IList<OrderUnit> changes, OrderUnitKeyedCollection<OrderUnit> pieces)
        {
            foreach (var change in changes)
            {
                decimal cp = change.Price;
                if (cp == 0) return;

                if (change.Size == 0.0M)
                {
                    pieces.Remove(cp);
                }
                else
                {
                    if (pieces.ContainsKey(cp))
                    {
                        var piece = pieces[cp];

                        piece.Size = change.Size;
                        piece.Sequence = change.Sequence;
                    }
                    else
                    {
                        var newPiece = new OrderUnit
                        {
                            Price = change.Price,
                            Size = change.Size,
                            Sequence = change.Sequence
                        };

                        pieces.Add(newPiece);
                    }
                }
            }
        }

        void IObserver<MatchExecution>.OnCompleted()
        {
        }

        void IObserver<MatchExecution>.OnError(Exception error)
        {
        }

        void IObserver<MatchExecution>.OnNext(MatchExecution value)
        {

            if (value.Symbol == Symbol)
            {
                MatchTotal++;
                matchSec++;

                Candle.Volume += (value.Price * value.Size);
            }
        }
    }

}
