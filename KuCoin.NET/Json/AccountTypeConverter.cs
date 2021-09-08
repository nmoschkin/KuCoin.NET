using KuCoin.NET.Data.User;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Json
{

    public class AccountTypeConverter : JsonConverter<AccountType>
    {
        public override AccountType ReadJson(JsonReader reader, Type objectType, AccountType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (AccountType)(string)reader.Value?.ToString().ToLower();
        }

        public override void WriteJson(JsonWriter writer, AccountType value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }
    }

}
