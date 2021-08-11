using Kucoin.NET.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kucoin.NET.Services
{
    public class ServiceFactory : IServiceFactory
    {
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

        public ISymbolDataService AddOrChangeSymbol(string symbol, ISymbolDataService current, bool sharedConnection)
        {

            if (current == null)
            {
                current = new SymbolDataService();
            }

            if (!current.Connected)
            {
                return null;
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
