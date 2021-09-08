using KuCoin.NET.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoinConsole.Converters
{
    public class AttachedAccountConverter : JsonConverter<ICredentialsProvider>
    {
        public override ICredentialsProvider ReadJson(JsonReader reader, Type objectType, ICredentialsProvider existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null && reader.TokenType != JsonToken.StartObject) return null;
            else return (ICredentialsProvider)serializer.Deserialize(reader, typeof(CryptoCredentials));
        }

        public override void WriteJson(JsonWriter writer, ICredentialsProvider value, JsonSerializer serializer)
        {
            if (value is CryptoCredentials cred)
            {
                serializer.Serialize(writer, value);
            }
            else
            {
                return;
            }
        }
    }
}
