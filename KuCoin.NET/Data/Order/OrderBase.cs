using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Data.Market;
using KuCoin.NET.Json;

using Newtonsoft.Json;

namespace KuCoin.NET.Data.Order
{
    /// <summary>
    /// Base class for all orders.
    /// </summary>
    public abstract class OrderBase : DataObject, IStreamableObject
    {
        protected string remark;

        /// <summary>
        /// Unique order id created by users to identify their orders, e.g. UUID.
        /// </summary>
        [JsonProperty("clientOid")]
        public string ClientOid { get; set; } = Guid.NewGuid().ToString("d");

        /// <summary>
        /// buy or sell
        /// </summary>
        [JsonProperty("side")]
        public Side Side { get; set; }

        /// <summary>
        /// a valid trading symbol code. e.g. ETH-BTC
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// [Optional] limit or market (default is limit)
        /// </summary>
        [JsonProperty("type")]
        public OrderType? Type { get; set; }

        /// <summary>
        /// [Optional] remark for the order, length cannot exceed 100 utf8 characters
        /// </summary>
        [JsonProperty("remark")]
        public virtual string Remark
        {
            get => remark;
            set
            {
                if (value?.Length > 100)
                {
                    throw new InvalidOperationException("Remark cannot exceed 100 characters in length.");
                }
                else
                {
                    remark = value;
                }
            }
        }

        /// <summary>
        /// [Optional] self trade prevention , CN, CO, CB or DC
        /// </summary>
        [JsonProperty("stp")]
        public StpMode? StpMode { get; set; }

        /// <summary>
        /// [Optional] The type of trading : TRADE（Spot Trade）, MARGIN_TRADE (Margin Trade). 
        /// </summary>
        [JsonProperty("tradeType")]
        public TradeType? TradeType { get; set; }

        #region For Margin Orders 

        /// <summary>
        /// [Optional] The type of trading, including cross (cross mode) and isolated (isolated mode). It is set at cross by default.
        /// </summary>
        [JsonProperty("marginMode")]
        public MarginMode? MarginMode { get; set; }

        /// <summary>
        /// Auto-borrow to place order. The system will first borrow you funds at the optimal interest rate and then place an order for you.
        /// </summary>
        [JsonProperty("autoBorrow")]
        public bool? AutoBorrow { get; set; }

        #endregion

    }
}
