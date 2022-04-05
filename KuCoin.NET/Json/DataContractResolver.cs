using System;
using System.Collections.Generic;
using System.Text;

using KuCoin.NET.Data.Market;

using Newtonsoft.Json.Serialization;

namespace KuCoin.NET.Json
{
    /// <summary>
    /// Global default data contract resolver for the KuCoin API.
    /// </summary>
    public class DataContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static readonly DataContractResolver Instance = new DataContractResolver();

        private static readonly StringToDecimalConverter decConv = new StringToDecimalConverter();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(decimal) || objectType == typeof(decimal?))
            {
                contract.Converter = decConv;
            }

            return contract;
        }
    }
}
