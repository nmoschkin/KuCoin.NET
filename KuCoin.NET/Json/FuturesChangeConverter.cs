using KuCoin.NET.Futures.Data.Market;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Json
{
    public class FuturesChangeConverter : JsonConverter<FuturesChange>
    {
        public override FuturesChange ReadJson(JsonReader reader, Type objectType, FuturesChange existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new FuturesChange((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, FuturesChange value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
