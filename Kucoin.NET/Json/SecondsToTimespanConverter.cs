using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Kucoin.NET.Json
{
    public class SecondsToTimespanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(long);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is long l)
            {
                return new TimeSpan(l * 10_000_000);
            }
            else if (reader.Value is string s)
            {
                return new TimeSpan(long.Parse(s) * 10_000_000);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TimeSpan ts)
            {
                writer.WriteValue((long)ts.TotalSeconds);
            }
            else
            {
                throw new NotImplementedException();
            }

        }
    }
}
