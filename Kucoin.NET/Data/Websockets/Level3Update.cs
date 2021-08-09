using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;
using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Data.Websockets
{
    /// <summary>
    /// Order completion reasons
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<DoneReason>))]
    public enum DoneReason
    {
        /// <summary>
        /// The order was filled
        /// </summary>
        [EnumMember(Value="filled")]
        Filled,

        /// <summary>
        /// The order was canceled
        /// </summary>
        [EnumMember(Value = "canceled")]
        Canceled
    }


    ///// <summary>
    ///// Level 3 Match Engine Update
    ///// </summary>
    //public struct Level3Update : ILevel3Update
    //{
    //    /// <summary>
    //    /// Sequence
    //    /// </summary>
    //    [JsonProperty("sequence")]
    //    public long Sequence { get; set; }

    //    /// <summary>
    //    /// Symbol
    //    /// </summary>
    //    [JsonProperty("symbol")]
    //    public string Symbol { get; set; }

    //    /// <summary>
    //    /// OrderId
    //    /// </summary>
    //    [JsonProperty("orderId")]
    //    public string OrderId { get; set; }


    //    /// <summary>
    //    /// ClientOid
    //    /// </summary>
    //    [JsonProperty("clientOid")]
    //    public string ClientOid { get; set; }


    //    /// <summary>
    //    /// Side
    //    /// </summary>
    //    [JsonProperty("side")]
    //    public Side? Side { get; set; }


    //    /// <summary>
    //    /// Price
    //    /// </summary>
    //    [JsonProperty("price")]
    //    public decimal? Price { get; set; }


    //    /// <summary>
    //    /// Size
    //    /// </summary>
    //    [JsonProperty("size")]
    //    public decimal? Size { get; set; }


    //    /// <summary>
    //    /// RemainSize
    //    /// </summary>
    //    [JsonProperty("remainSize")]
    //    public decimal? RemainSize { get; set; }


    //    /// <summary>
    //    /// TakerOrderId
    //    /// </summary>
    //    [JsonProperty("takerOrderId")]
    //    public string TakerOrderId { get; set; }


    //    /// <summary>
    //    /// MakerOrderId
    //    /// </summary>
    //    [JsonProperty("makerOrderId")]
    //    public string MakerOrderId { get; set; }


    //    /// <summary>
    //    /// TradeId
    //    /// </summary>
    //    [JsonProperty("tradeId")]
    //    public string TradeId { get; set; }

    //    /// <summary>
    //    /// Done Reason
    //    /// </summary>
    //    [JsonProperty("reason")]
    //    public DoneReason? Reason { get; set; }


    //    /// <summary>
    //    /// Order time
    //    /// </summary>
    //    [JsonProperty("orderTime")]
    //    [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
    //    public DateTime? OrderTime { get; set; }


    //    /// <summary>
    //    /// Time Stamp
    //    /// </summary>
    //    [JsonProperty("ts")]
    //    [JsonConverter(typeof(AutoTimeConverter), TimeTypes.InNanoseconds)]
    //    public DateTime? Timestamp { get; set; }


    //    [JsonIgnore]
    //    public string Subject { get; set; }


    //}



    /// <summary>
    /// Level 3 Match Engine Update
    /// </summary>
    public struct Level3Update : ILevel3Update
    {

        public string symbol { get; set; }

        /// <summary>
        /// Symbol
        /// </summary>
        [JsonIgnore]
        public string Symbol { get => symbol; set => symbol = value; }
                
        public long sequence { get; set; }

        /// <summary>
        /// Sequence
        /// </summary>
        [JsonIgnore]
        public long Sequence { get => sequence; set => sequence = value; }

        public string orderId { get; set; }

        /// <summary>
        /// MakerOrderId
        /// </summary>
        [JsonIgnore]
        public string OrderId { get => orderId; set => orderId = value; }


        public string clientOid { get; set; }

        /// <summary>
        /// MakerOrderId
        /// </summary>
        [JsonIgnore]
        public string ClientOid { get => clientOid; set => clientOid = value; }


        private string s_side;

        public string side
        {
            get => s_side;
            set
            {
                s_side = value;
                m_side = null;
            }
        }

        private Side? m_side;

        /// <summary>
        /// Side
        /// </summary>
        [JsonIgnore]
        public Side? Side
        {
            get
            {
                if (m_side is Side sd)
                {
                    return sd;
                }
                else
                {
                    if (side != null)
                    {
                        m_side = EnumToStringConverter<Side>.GetEnumValue<Side>(side);
                        return m_side;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }




        private string s_price;

        public string price
        {
            get => s_price;
            set
            {
                s_price = value;
                m_price = null;
            }
        }

        private decimal? m_price;

        /// <summary>
        /// Price
        /// </summary>
        [JsonIgnore]
        public decimal? Price
        {
            get
            {
                if (m_price is decimal de)
                {
                    return de;
                }
                else
                {
                    if (!string.IsNullOrEmpty(price))
                    {
                        var dn = decimal.Parse(price);
                        m_price = dn;
                        return dn;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }




        private string s_size;

        public string size
        {
            get => s_size;
            set
            {
                s_size = value;
                m_size = null;
            }
        }

        private decimal? m_size;

        /// <summary>
        /// Size
        /// </summary>
        [JsonIgnore]
        public decimal? Size
        {
            get
            {
                if (m_size is decimal de)
                {
                    return de;
                }
                else
                {
                    if (size != null)
                    {
                        var dn = decimal.Parse(size);
                        m_size = dn;
                        return dn;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }




        private string s_remainSize;

        public string remainSize
        {
            get => s_remainSize;
            set
            {
                s_remainSize = value;
                m_remainSize = null;
            }
        }

        private decimal? m_remainSize;

        /// <summary>
        /// RemainSize
        /// </summary>
        [JsonIgnore]
        public decimal? RemainSize
        {
            get
            {
                if (m_remainSize is decimal de)
                {
                    return de;
                }
                else
                {
                    if (remainSize != null)
                    {
                        var dn = decimal.Parse(remainSize);
                        m_remainSize = dn;
                        return dn;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private string s_reason;

        public string reason
        {
            get => s_reason;
            set
            {
                s_reason = value;
                m_reason = null;
            }
        }

        private DoneReason? m_reason;

        /// <summary>
        /// Done Reason
        /// </summary>
        [JsonIgnore]
        public DoneReason? Reason
        {
            get
            {
                if (m_reason is DoneReason dr)
                {
                    return dr;
                }
                else
                {
                    if (reason != null)
                    {
                        m_reason = EnumToStringConverter<DoneReason>.GetEnumValue<DoneReason>(reason);
                        return m_reason;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                m_reason = value;
                s_reason = EnumToStringConverter<DoneReason>.GetEnumName(m_reason);
            }
        }




        public string takerOrderId { get; set; }

        /// <summary>
        /// MakerOrderId
        /// </summary>
        [JsonIgnore]
        public string TakerOrderId { get => takerOrderId; set => takerOrderId = value; }

        public string makerOrderId { get; set; }

        /// <summary>
        /// MakerOrderId
        /// </summary>
        [JsonIgnore]
        public string MakerOrderId { get => makerOrderId; set => makerOrderId = value; }



        public string tradeId { get; set; }

        /// <summary>
        /// TradeId
        /// </summary>
        [JsonIgnore]
        public string TradeId { get => tradeId; set => tradeId = value; }

        public long? orderTime { get; set; }

        private DateTime? m_orderTime;

        /// <summary>
        /// OrderTime
        /// </summary>
        [JsonIgnore]
        public DateTime? OrderTime
        {
            get
            {
                if (m_orderTime is DateTime dt)
                {
                    return dt;
                }
                else
                {
                    if (orderTime != null)
                    {
                        m_orderTime = EpochTime.NanosecondsToDate((long)orderTime);
                        return m_orderTime;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
        }

        public long? ts { get; set; }

        private DateTime? m_ts;

        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonIgnore]
        public DateTime? Timestamp
        {
            get
            {
                if (m_ts is DateTime dt)
                {
                    return dt;
                }
                else
                {
                    if (ts != null)
                    {
                        m_ts = EpochTime.NanosecondsToDate((long)ts);
                        return m_ts;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
        }




        [JsonIgnore]
        public string Subject { get; set; }

    }


}
