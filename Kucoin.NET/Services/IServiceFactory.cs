using Kucoin.NET.Helpers;

namespace Kucoin.NET.Services
{
    public interface IServiceFactory
    {
        ISymbolDataService AddOrChangeSymbol(string symbol, ISymbolDataService current, bool sharedConnection);
        ISymbolDataService CreateConnected(ICredentialsProvider credentialsProvider);
        ISymbolDataService CreateAssigned(string symbol, ICredentialsProvider credentialsProvider);
    }
}