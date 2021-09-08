
using System;
using System.Collections.Generic;
using System.Text;

namespace KuCoin.NET.Helpers
{

    /// <summary>
    /// Defines an interface for retrieving REST API sign-in credentials.
    /// </summary>
    public interface ICredentialsProvider
    {
        /// <summary>
        /// Get the API Key
        /// </summary>
        /// <returns></returns>
        string GetKey();

        /// <summary>
        /// Get the API Secret
        /// </summary>
        /// <returns></returns>
        string GetSecret();

        /// <summary>
        /// Get the API Passphrase
        /// </summary>
        /// <returns></returns>
        string GetPassphrase();

        /// <summary>
        /// Gets a value indicating if these credentials are for a sandbox account.
        /// </summary>
        /// <returns></returns>
        bool GetSandbox();


        /// <summary>
        /// Gets a value indicating if these credentials are for a KuCoin Futures account.
        /// </summary>
        /// <returns></returns>
        bool GetFutures();

        /// <summary>
        /// Gets another credentials provider that is attached to this one (for example, the Futures API.)
        /// </summary>
        ICredentialsProvider AttachedAccount { get; }

    }
}
