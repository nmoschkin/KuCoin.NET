using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KuCoin.NET.Json
{
    public class StringToJsonObjectConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(JToken) || objectType == typeof(JObject);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                var jobj = JsonConvert.DeserializeObject<JObject>(s);
                return jobj.ToObject<T>();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize(reader, objectType);
            }
            else if (reader.Value == null)
            {
                return null;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is T v)
            {
                writer.WriteValue(JsonConvert.SerializeObject(v));
            }
            else if (value != null)
            {
                throw new ArgumentException();
            }
        }
    }
}
