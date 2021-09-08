using KuCoin.NET.Helpers;
using KuCoin.NET.Rest;

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace KuCoin.NET.Futures.Rest
{
    /// <summary>
    /// Base class for futures REST API classes
    /// </summary>
    public abstract class FuturesBaseRestApi : KucoinBaseRestApi
    {

        /// <summary>
        /// Default Constructor (Futures API)
        /// </summary>
        /// <param name="credProvider">An object that implements <see cref="ICredentialsProvider"/>.</param>
        public FuturesBaseRestApi(ICredentialsProvider credProvider) : base(credProvider, futures: true)
        {
            if (credProvider != null && credProvider.GetFutures() != true)
            {
                throw new InvalidCredentialException("Must be marked as Futures credentials.");
            }
        }

        /// <summary>
        /// Default Constructor (Futures API)
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="secret">API Secret</param>
        /// <param name="passphrase">API Passphrase</param>
        /// <param name="isSandbox">Is sandbox / not real-time.</param>
        public FuturesBaseRestApi(
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
