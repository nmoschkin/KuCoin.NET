using System;
using System.Collections.Generic;
using System.Text;

using Kucoin.NET.Data.Market;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kucoin.NET.Json
{
    public class DataContractResolver : DefaultContractResolver
    {
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
