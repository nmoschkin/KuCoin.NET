using Kucoin.NET.Data.Interfaces;
using Kucoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Futures.Rest
{
    public class User : KucoinBaseRestApi
    {
        public User(ICredentialsProvider cred) : base(cred, futures: true)
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
                isSandbox: 
                isSandbox, 
                futures: true)
        {



        }


    }
}
