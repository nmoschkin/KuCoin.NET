using KuCoin.NET.Data.Market;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Json
{
    public class OrderUnitConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(OrderUnit).IsAssignableFrom(objectType) || typeof(OrderUnitStruct).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {

                if (objectType == typeof(OrderUnitStruct))
                {
                    return new OrderUnitStruct((string[])serializer.Deserialize(reader, typeof(string[])));
                }
                else if (objectType == typeof(OrderUnit))
                {
                    return new OrderUnit((string[])serializer.Deserialize(reader, typeof(string[])));
                }
                else if (objectType == typeof(ObservableOrderUnit))
                {
                    return new ObservableOrderUnit((string[])serializer.Deserialize(reader, typeof(string[])));
                }
                else
                {
                    return Activator.CreateInstance(objectType, new object[] { serializer.Deserialize(reader, typeof(string[])) });
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string[] str;

            if (value is IOrderUnit o)
            {
                if (o is ISequencedOrderUnit so)
                {
                    str = new string[] { so.Price.ToString(), so.Size.ToString(), so.Sequence.ToString() };
                }
                else
                {
                    str = new string[] { o.Price.ToString(), o.Size.ToString() };
                }

                writer.WriteValue(str);
            }
            else
            {
                throw new NotImplementedException();
            }

        }
    }
}
