using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Json
{
    public class AtomicOrderUnitConverter : JsonConverter<AtomicOrderUnit>
    {
        public override AtomicOrderUnit ReadJson(JsonReader reader, Type objectType, AtomicOrderUnit existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return new AtomicOrderUnit((object[])serializer.Deserialize(reader, typeof(object[])));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, AtomicOrderUnit value, JsonSerializer serializer)
        {
            object[] str;

            if (value.OrderId != null)
            {
                str = new object[] { value.OrderId, value.Price.ToString(), value.Size.ToString(), EpochTime.DateToNanoseconds(value.Timestamp) };
            }
            else
            {
                str = new object[] { value.Price.ToString(), value.Size.ToString() };
            }

            writer.WriteValue(str);
        }
    }
}
