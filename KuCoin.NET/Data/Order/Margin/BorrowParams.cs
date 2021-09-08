using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Data.Order.Margin
{
    /// <summary>
    /// Borrow order execution type
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<BorrowType>))]
    public enum BorrowType
    {
        /// <summary>
        /// Fill the order completely or do not fill it at all
        /// </summary>
        [EnumMember(Value = "FOK")]
        FillOrKill,

        /// <summary>
        /// Fill the order immediately or cancel the order
        /// </summary>
        [EnumMember(Value = "IOC")]
        ImmediateOrCancel
    }

    /// <summary>
    /// Borrow parameters class
    /// </summary>
    public class BorrowParams : DataObject
    {
        /// <summary>
        /// Currency to Borrow
        /// </summary>
        [JsonProperty("currency")]
        public string currency { get; set; }

        /// <summary>
        /// Type: FOK, IOC
        /// </summary>
        [JsonProperty("type")]
        public BorrowType Type { get; set; }

        /// <summary>
        /// Total size
        /// </summary>
        [JsonProperty("size")]
        public decimal Size { get; set; }

        /// <summary>
        /// [Optional] The max interest rate. All interest rates are acceptable if this field is left empty.
        /// </summary>
        [JsonProperty("maxRate")]
        public decimal MaxRate { get; set; }

        /// <summary>
        /// [Optional] Term (Unit: Day). All terms are acceptable if this field is left empty. Please note to separate the terms via comma. For example, 7,14,28.
        /// </summary>
        [JsonProperty("term")]
        internal string InternalTerm { get; set; }

        /// <summary>
        /// [Optional] Term (Unit: Day). All terms are acceptable if this field is left empty. 
        /// </summary>
        public int[] Term
        {
            get
            {
                if (string.IsNullOrEmpty(InternalTerm)) return null;

                var sp = InternalTerm.Split(',');
                List<int> ints = new List<int>();
                foreach (var s in sp)
                {
                    ints.Add(int.Parse(s));
                }
                return ints.ToArray();
            }
            set
            {
                if (value == null)
                {
                    InternalTerm = null;
                    return;
                }

                var sb = new StringBuilder();
                foreach (var i in value)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(i.ToString());
                }

                InternalTerm = sb.ToString();

            }
        }



    }
}
