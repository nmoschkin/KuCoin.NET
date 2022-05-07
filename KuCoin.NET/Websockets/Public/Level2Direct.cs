using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuCoin.NET.Websockets.Public
{
    public class Level2Direct : Level2
    {

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="credentialsProvider">API Credentials.</param>
        public Level2Direct(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
        {
            if (credentialsProvider.GetFutures()) throw new NotSupportedException("Cannot use Futures API credentials on a spot market feed.");

            recvBufferSize = 4194304;
            chunkSize = 1024;
        }

        /// <summary>
        /// Instantiate a new market feed.
        /// </summary>
        /// <param name="key">API key.</param>
        /// <param name="secret">API secret.</param>
        /// <param name="passphrase">API passphrase.</param>
        /// <param name="isSandbox">True if sandbox mode.</param>
        public Level2Direct(string key, string secret, string passphrase, bool isSandbox = false) : base(key, secret, passphrase, isSandbox: isSandbox)
        {
            recvBufferSize = 4194304;
            chunkSize = 1024;
        }

    }
}
