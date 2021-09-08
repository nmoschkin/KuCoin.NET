using KuCoin.NET.Helpers;

namespace KuCoin.NET.Services
{
    /// <summary>
    /// <see cref="ISymbolDataService"/> factory interface.
    /// </summary>
    public interface IServiceFactory
    {

        /// <summary>
        /// Creates, connects, and assigns a new <see cref="ISymbolDataService"/> object.
        /// </summary>
        /// <param name="symbol">The symbol to enable.</param>
        /// <param name="credentialsProvider">The <see cref="ICredentialsProvider"/> to sign in with.</param>
        /// <returns>A new <see cref="ISymbolDataService"/> object that has been initialized, connected, and assigned, or null on failure.</returns>
        ISymbolDataService CreateAssigned(string symbol, ICredentialsProvider credentialsProvider);

        /// <summary>
        /// Creates and connects a new <see cref="ISymbolDataService"/> object.
        /// </summary>
        /// <param name="credentialsProvider">The <see cref="ICredentialsProvider"/> to sign in with.</param>
        /// <returns>A new <see cref="ISymbolDataService"/> object that has been initialized and connected, or null on failure.</returns>
        ISymbolDataService CreateConnected(ICredentialsProvider credentialsProvider);

        /// <summary>
        /// Enable or add a symbol using an instance of <see cref="ISymbolDataService"/>.
        /// </summary>
        /// <param name="symbol">The symbol to enable.</param>
        /// <param name="current">The current <see cref="ISymbolDataService"/> instance.</param>
        /// <param name="sharedConnection">True to share the connection.</param>
        /// <returns>A <see cref="ISymbolDataService"/> object connected to the specified symbol.</returns>
        /// <remarks>
        /// If the current service is not assigned a symbol, then the symbol will be assigned and the current service is returned.
        /// If it is assigned a symbol, then a new instance of <see cref="ISymbolDataService"/> will be created, assigned the symbol, and returned.
        /// </remarks>
        ISymbolDataService EnableOrAddSymbol(string symbol, ISymbolDataService current, bool sharedConnection);
    }
}