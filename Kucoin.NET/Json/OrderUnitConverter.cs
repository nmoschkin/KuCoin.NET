using Kucoin.NET.Data.Market;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Json
{
    public class OrderUnitConverter : JsonConverter
    {
        //public override OrderUnit ReadJson(JsonReader reader, Type objectType, OrderUnit existingValue, bool hasExistingValue, JsonSerializer serializer)
        //{
        //    if (reader.TokenType == JsonToken.StartArray)
        //    {
        //        return new OrderUnit((string[])serializer.Deserialize(reader, typeof(string[])));
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override void WriteJson(JsonWriter writer, OrderUnit value, JsonSerializer serializer)
        //{
        //    string[] str;

        //    if (value.Sequence != 0)
        //    {
        //        str = new string[] { value.Price.ToString(), value.Size.ToString(), value.Sequence.ToString() };
        //    }
        //    else
        //    {
        //        str = new string[] { value.Price.ToString(), value.Size.ToString() };
        //    }

        //    writer.WriteValue(str);
        //}
        public override bool CanConvert(Type objectType)
        {
            return typeof(OrderUnit).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                if (objectType == typeof(OrderUnit))
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

            if (value is OrderUnit o)
            {

                if (o.Sequence != 0)
                {
                    str = new string[] { o.Price.ToString(), o.Size.ToString(), o.Sequence.ToString() };
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
