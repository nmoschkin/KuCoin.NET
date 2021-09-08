using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using KuCoin.NET.Localization;
using Newtonsoft.Json;
using KuCoin.NET.Json;

namespace KuCoin.NET.Data.User
{

    /// <summary>
    /// Account types
    /// </summary>
    [JsonConverter(typeof(AccountTypeConverter))]
    public struct AccountType
    {
        private string value;

        public string AccountName => value;

        private AccountType(string type)
        {
            value = type;
        }

        /// <summary>
        /// Invalid account
        /// </summary>
        public static readonly AccountType Invalid = new AccountType(null);

        /// <summary>
        /// Main account (deposits/withdrawals)
        /// </summary>
        public static readonly AccountType Main = new AccountType("main");
        
        /// <summary>
        /// Trading account (spot)
        /// </summary>
        public static readonly AccountType Trading = new AccountType("trade");

        /// <summary>
        /// Pool-X account
        /// </summary>
        public static readonly AccountType PoolX = new AccountType("pool");

        /// <summary>
        /// Trading account (margin)
        /// </summary>
        public static readonly AccountType Margin = new AccountType("margin");

        /// <summary>
        /// Futures account
        /// </summary>
        public static readonly AccountType Futures = new AccountType("futures");

        /// <summary>
        /// Contract account
        /// </summary>
        public static readonly AccountType Contract = new AccountType("contract");

        /// <summary>
        /// Parse the account type from the given string
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>An account type</returns>
        public static AccountType Parse(string s)
        {
            return (AccountType)s;
        }

        /// <summary>
        /// Try to parse the account type from the given string.
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <param name="obj">The returned AccountType</param>
        /// <returns>True if successful</returns>
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

        /// <summary>
        /// Returns a descriptive string for the account type
        /// </summary>
        /// <returns></returns>
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
