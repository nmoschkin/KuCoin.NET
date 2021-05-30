using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
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
        /// <param name="currency">The currency of the account to retrieve (or null for all currencies.)</param>
        /// <returns></returns>
        public async Task GetAccountsOverview(string currency = null)
        {
            var dict = new Dictionary<string, object>();

            if (currency != null)
            {
                dict.Add("currency", currency);

            }

            var url = "/api/v1/contracts/active";




        }


    }
}
