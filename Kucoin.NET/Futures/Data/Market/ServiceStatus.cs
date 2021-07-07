using Kucoin.NET.Json;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kucoin.NET.Futures.Data.Market
{
    [JsonConverter(typeof(EnumToStringConverter<ServiceStatus>))]
    public enum ServiceStatus
    {
        [EnumMember(Value = "open")]
        Open,

        [EnumMember(Value = "close")]
        Close,

        [EnumMember(Value = "cancelonly")]
        CancelOnly
    }

    public class ServiceInfo
    {

        [JsonProperty("status")]
        public ServiceStatus Status { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

    }


}
