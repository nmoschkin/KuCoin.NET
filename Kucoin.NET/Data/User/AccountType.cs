using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Kucoin.NET.Localization;
using Newtonsoft.Json;

namespace Kucoin.NET.Data.User
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

    [JsonConverter(typeof(AccountTypeConverter))]
    public struct AccountType
    {
        private string value;

        public string AccountName => value;

        private AccountType(string type)
        {
            value = type;
        }

        public static readonly AccountType Invalid = new AccountType(null);

        public static readonly AccountType Main = new AccountType("main");
        
        public static readonly AccountType Trading = new AccountType("trade");

        public static readonly AccountType PoolX = new AccountType("pool");

        public static readonly AccountType Margin = new AccountType("margin");

        public static readonly AccountType Futures = new AccountType("futures");

        public static readonly AccountType Contract = new AccountType("contract");

        public static AccountType Parse(string s)
        {
            return (AccountType)s;
        }

        public static bool TryParse(string s, out AccountType obj)
        {
            try
            {
                obj = (AccountType)s;
                return true;
            }
            catch
            {
                obj = AccountType.Invalid;
                return false;
            }

            
        }

        public override string ToString()
        {
            return AutoLocalizer.Localize("Account.Type", value);
        }

        public static implicit operator string(AccountType val)
        {
            return val.value;
        }
        public override int GetHashCode()
        {
            return value?.GetHashCode() ?? 0;
        }

        public static explicit operator AccountType(string val)
        {
            var t = typeof(AccountType);

            var fis = t.GetFields(BindingFlags.Public | BindingFlags.Static);
            var search = val?.ToLower() ?? null;

            foreach (var fi in fis)
            {
                if (fi.FieldType == t)
                {
                    var x = (AccountType)fi.GetValue(null);
                    var inspect = x.value?.ToLower() ?? null;

                    if (inspect == search)
                    {
                        return x;
                    }
                }
            }

            throw new ArgumentOutOfRangeException();
            //return new AccountType(val);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s)
            {
                return s == value;
            }
            else if (obj is AccountType t)
            {
                return t.value == value;
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(AccountType val1, AccountType val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(AccountType val1, AccountType val2)
        {
            return !val1.Equals(val2);
        }

        public static bool operator ==(string val1, AccountType val2)
        {
            return val2.Equals(val1);
        }

        public static bool operator !=(string val1, AccountType val2)
        {
            return !val2.Equals(val1);
        }


        public static bool operator ==(AccountType val1, string val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(AccountType val1, string val2)
        {
            return !val1.Equals(val2);
        }


    }
}
