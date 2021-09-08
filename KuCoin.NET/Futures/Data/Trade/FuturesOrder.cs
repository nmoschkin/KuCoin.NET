using KuCoin.NET.Data;
using KuCoin.NET.Data.Market;
using KuCoin.NET.Data.Order;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Futures.Data.Trade
{

    /// <summary>
    /// Describes a futures order
    /// </summary>
    public abstract class FuturesOrderBase : DataObject, IDataObject, ISymbol
    {
        /// <summary>
        /// Unique order id created by users to identify their orders, e.g. UUID, Only allows numbers, characters, underline(_), and separator(-)
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; } = Guid.NewGuid().ToString("d");

        /// <summary>
        /// buy or sell
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }

        /// <summary>
        /// a valid contract code. e.g. XBTUSDM
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// [optional] Either limit or market
        /// </summary>
        [JsonProperty("type")]
        public virtual FuturesOrderType Type { get; set; } 

        /// <summary>
        /// Leverage of the order
        /// </summary>
        [JsonProperty("leverage")]
        public decimal? Leverage { get; set; }

        /// <summary>
        /// [optional] remark for the order, length cannot exceed 100 utf8 characters
        /// </summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }

        /// <summary>
        /// [optional] Either down or up. Requires stopPrice and stopPriceType to be defined
        /// </summary>
        [JsonProperty("stop")]
        public FuturesStopType? Stop { get; set; }

        /// <summary>
        /// [optional] Either TP, IP or MP, Need to be defined if stop is specified.
        /// </summary>
        [JsonProperty("stopPriceType")]
        public FuturesStopPriceType? StopPriceType { get; set; }

        /// <summary>
        /// [optional] Need to be defined if stop is specified.
        /// </summary>
        [JsonProperty("stopPrice")]
        public decimal? StopPrice { get; set; }



        /// <summary>
        /// [optional] A mark to reduce the position size only. Set to false by default. Need to set the position size when reduceOnly is true.
        /// </summary>
        [JsonProperty("reduceOnly")]
        public bool? ReduceOnly { get; set; }



        /// <summary>
        /// [optional] A mark to close the position. Set to false by default. It will close all the positions when closeOrder is true.
        /// </summary>
        [JsonProperty("closeOrder")]
        public bool? CloseOrder { get; set; }

        /// <summary>
        /// [optional] A mark to forcely hold the funds for an order, even though it's an order to reduce the position size. This helps the order stay on the order book and not get canceled when the position size changes. Set to false by default.
        /// </summary>
        [JsonProperty("forceHold")]
        public bool? ForceHold { get; set; }
    }

    public class MarketFuturesOrder : FuturesOrderBase
    {

        /// <summary>
        /// [optional] amount of contract to buy or sell
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

        [JsonProperty("type")]
        public override FuturesOrderType Type { get; set; } = FuturesOrderType.Market;
    }

    public class LimitFuturesOrder : FuturesOrderBase
    {

        /// <summary>
        /// Limit price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }



        /// <summary>
        /// Order size. Must be a positive number
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }



        /// <summary>
        /// [optional] GTC, IOC(default is GTC), read Time In Force
        /// </summary>
        [JsonProperty("timeInForce")]
        public FuturesTimeInForce? TimeInForce { get; set; }



        /// <summary>
        /// [optional] Post only flag, invalid when timeInForce is IOC. When postOnly chose, not allowed choose hidden or iceberg.
        /// </summary>
        [JsonProperty("postOnly")]
        public bool? PostOnly { get; set; }



        /// <summary>
        /// [optional] Orders not displaying in order book. When hidden chose, not allowed choose postOnly.
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }



        /// <summary>
        /// [optional] Only visible portion of the order is displayed in the order book. When iceberg chose, not allowed choose postOnly.
        /// </summary>
        [JsonProperty("iceberg")]
        public bool? Iceberg { get; set; }



        /// <summary>
        /// [optional] The maximum visible size of an iceberg order
        /// </summary>
        [JsonProperty("visibleSize")]
        public decimal? VisibleSize { get; set; }

        [JsonProperty("type")]
        public override FuturesOrderType Type { get; set; } = FuturesOrderType.Limit;


    }


}
