using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kucoin.NET.Futures.Rest
{
    public class User : FuturesBaseRestApi
    {
        public User(ICredentialsProvider cred) : base(cred)
        {
        }

        public User(
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
