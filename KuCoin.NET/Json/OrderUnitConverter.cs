using KuCoin.NET.Data.Market;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Json
{    
    public class OrderUnitConverter<T> : JsonConverter<T> where T : IOrderUnit, new()
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.StartArray)
            {
                string[] vals = new string[3];
                var x = new T();
                string s;

                reader.Read();
                s = reader.Value.ToString();

                if (s.Contains('E'))
                {
                    x.Price = (decimal)double.Parse(s);
                }
                else
                {
                    x.Price = decimal.Parse(s);
                }

                reader.Read();

                x.Size = decimal.Parse(reader.Value.ToString());

                reader.Read();

                if (x is ISequencedOrderUnit sq && reader.TokenType != JsonToken.EndArray)
                {
                    sq.Sequence = long.Parse(reader.Value.ToString());
                    reader.Read();
                }

                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();
                }

                return x;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Price.ToString());
            writer.WriteValue(value.Size.ToString());

            if (value is ISequencedOrderUnit sq)
            {
                writer.WriteValue(sq.Sequence.ToString());
            }

            writer.WriteEndArray();
        }
    }

}
