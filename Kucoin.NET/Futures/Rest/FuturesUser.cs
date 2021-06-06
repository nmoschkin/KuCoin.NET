using Kucoin.NET.Data.User;
using Kucoin.NET.Futures.Data.User;
using Kucoin.NET.Helpers;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Rest
{
    /// <summary>
    /// Futures user account information
    /// </summary>
    public class FuturesUser : FuturesBaseRestApi
    {
        public FuturesUser(ICredentialsProvider cred) : base(cred)
        {
        }

        public FuturesUser(
            string key, 
            string secret, 
            string passphrase, 
            bool isSandbox = false) 
            : base(
                key, 
                secret, 
                passphrase, 
                isSandbox)
        {
        }

        /// <summary>
        /// Get accounts overview
        /// </summary>
        /// <param name="currency">The currency of the account to retrieve (default is XBT.)</param>
        /// <returns></returns>
        public async Task<FuturesAccount> GetAccountOverview(FuturesCurrency? currency = FuturesCurrency.XBT)
        {
            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);

            }

            var url = "/api/v1/account-overview";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<FuturesAccount>();
        }


        public async Task<AccountAddress> GetDepositAddress(FuturesCurrency? currency = FuturesCurrency.XBT)
        {
            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);

            }

            var url = "/api/v1/deposit-address";

            var jobj = await MakeRequest(HttpMethod.Get, url, reqParams: dict);

            return jobj.ToObject<AccountAddress>();

        }

    }
}
