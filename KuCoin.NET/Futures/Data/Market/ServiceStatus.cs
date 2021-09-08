using KuCoin.NET.Data;
using KuCoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KuCoin.NET.Futures.Data.Market
{
    /// <summary>
    /// KuCoin Futures Service status
    /// </summary>
    [JsonConverter(typeof(EnumToStringConverter<ServiceStatus>))]
    public enum ServiceStatus
    {
        /// <summary>
        /// KuCoin Futures is open for trading
        /// </summary>
        [EnumMember(Value = "open")]
        Open,

        /// <summary>
        /// KuCoin Futures is closed to trading
        /// </summary>
        [EnumMember(Value = "close")]
        Close,

        /// <summary>
        /// KuCoin Futures is open for canceling orders only.
        /// </summary>
        [EnumMember(Value = "cancelonly")]
        CancelOnly
    }

    /// <summary>
    /// KuCoin Futures Service Information
    /// </summary>
    public class ServiceInfo : DataObject
    {
        /// <summary>
        /// Gets the service status
        /// </summary>
        [JsonProperty("status")]
        public ServiceStatus Status { get; set; }

        /// <summary>
        /// Gets the service status message
        /// </summary>
        [JsonProperty("msg")]
        public string Message { get; set; }

    }


}
