using Kucoin.NET.Data.Market;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Json
{
    public class OrderUnitConverter : JsonConverter<OrderUnit>
    {
        public override OrderUnit ReadJson(JsonReader reader, Type objectType, OrderUnit existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return new OrderUnit((string[])serializer.Deserialize(reader, typeof(string[])));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, OrderUnit value, JsonSerializer serializer)
        {
            string[] str;
            
            if (value.Sequence != 0)
            {
                str = new string[] { value.Price.ToString(), value.Size.ToString(), value.Sequence.ToString() };
            }
            else
            {
                str = new string[] { value.Price.ToString(), value.Size.ToString() };
            }

            writer.WriteValue(str);
        }
    }
}
