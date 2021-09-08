using KuCoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace KuCoin.NET.Services
{
    /// <summary>
    /// <see cref="ISymbolDataService"/> factory implementation.
    /// </summary>
    public class ServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Gets the <see cref="IServiceFactory"/> instance for this application domain.
        /// </summary>
        public static readonly ServiceFactory Instance;

        static ServiceFactory()
        {
            Instance = new ServiceFactory();
        }

        private ServiceFactory()
        {
        }


        public ISymbolDataService CreateConnected(ICredentialsProvider credentialsProvider)
        {
            var sds = new SymbolDataService();
            var b = sds.Connect(credentialsProvider).ConfigureAwait(false).GetAwaiter().GetResult();

            if (b)
            {
                return sds;
            }
            else
            {
                return null;
            }

        }

        public ISymbolDataService CreateAssigned(string symbol, ICredentialsProvider credentialsProvider)
        {
            var sds = new SymbolDataService();
            var b = sds.Connect(credentialsProvider).ConfigureAwait(false).GetAwaiter().GetResult();

            if (b)
            {
                sds.ChangeSymbol(symbol).ConfigureAwait(false).GetAwaiter().GetResult();
                return sds;
            }
            else
            {
                return null;
            }

        }

        public ISymbolDataService EnableOrAddSymbol(string symbol, ISymbolDataService current, bool sharedConnection)
        {

            if (current == null)
            {
                current = new SymbolDataService();
            }

            if (!current.Connected)
            {
                try
                {
                    current.Reconnect(false).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {
                    return null;
                }
            }

            if (string.IsNullOrEmpty(current.Symbol))
            {
                current.ChangeSymbol(symbol);
                return current;
            }
            else
            {
                current = current.AddSymbol(symbol, sharedConnection).ConfigureAwait(false).GetAwaiter().GetResult();
                return current;
            }

        }

    }
}
