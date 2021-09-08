using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Newtonsoft.Json;


namespace KuCoin.NET.Json
{
    public class StringToDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }
            else if (reader.Value is string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    if (objectType == typeof(decimal))
                    {
                        return 0;
                    }
                    else if (objectType == typeof(decimal?))
                    {
                        return null;
                    }
                }

                return decimal.Parse(s);
            }
            else if (reader.Value is decimal de)
            {
                return (decimal)reader.Value;
            }
            else if (reader.Value is double du)
            {
                return (decimal)du;
            }
            else if (reader.Value is long l)
            {
                return (decimal)(double)l;
            }            
            else
            {
                try
                {
                    return (decimal)reader.Value;
                }
                catch // (Exception ex)
                {
                    throw new ArgumentException();
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;
            writer.WriteValue(value.ToString());
        }
    }
}
