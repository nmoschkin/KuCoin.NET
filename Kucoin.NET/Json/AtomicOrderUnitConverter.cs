using Kucoin.NET.Data.Market;
using Kucoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Json
{
    public class AtomicOrderUnitConverter : JsonConverter
    {

        //public override AtomicOrderUnit ReadJson(JsonReader reader, Type objectType, AtomicOrderUnit existingValue, bool hasExistingValue, JsonSerializer serializer)
        //{
        //    if (reader.TokenType == JsonToken.StartArray)
        //    {
        //        return new AtomicOrderUnit((object[])serializer.Deserialize(reader, typeof(object[])));
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override void WriteJson(JsonWriter writer, AtomicOrderUnit value, JsonSerializer serializer)
        //{
        //    object[] str;

        //    if (value.OrderId != null)
        //    {
        //        str = new object[] { value.OrderId, value.Price.ToString(), value.Size.ToString(), EpochTime.DateToNanoseconds(value.Timestamp) };
        //    }
        //    else
        //    {
        //        str = new object[] { value.Price.ToString(), value.Size.ToString() };
        //    }

        //    writer.WriteValue(str);
        //}
        public override bool CanConvert(Type objectType)
        {
            return typeof(AtomicOrderUnit).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                if (objectType == typeof(AtomicOrderUnit))
                {
                    return new AtomicOrderUnit((object[])serializer.Deserialize(reader, typeof(object[])));
                }
                else if (objectType == typeof(ObservableAtomicOrderUnit))
                {
                    return new ObservableAtomicOrderUnit((object[])serializer.Deserialize(reader, typeof(object[])));
                }
                else
                {
                    return Activator.CreateInstance(objectType, new object[] { serializer.Deserialize(reader, typeof(object[])) });
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is AtomicOrderUnit o)
            {
                object[] str;

                if (o.OrderId != null)
                {
                    str = new object[] { o.OrderId, o.Price.ToString(), o.Size.ToString(), EpochTime.DateToNanoseconds(o.Timestamp) };
                }
                else
                {
                    str = new object[] { o.Price.ToString(), o.Size.ToString() };
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
